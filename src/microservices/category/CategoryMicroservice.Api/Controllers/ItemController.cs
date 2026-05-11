using CategoryMicroservice.Api.DTOs.Items;
using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CategoryMicroservice.Api.Constants;
using CategoryMicroservice.Api.Services;
using MessageBus.Messages.Item;
using MessageBus.Messages.Saga.CreateItemWIthReview;
using RestrictionGrpcService;

namespace CategoryMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher, ILogger<ItemController> logger,
        ImageValidator imageValidator, RestrictionInfo.RestrictionInfoClient restrictionInfoClient) : ControllerBase
    {

        [HttpGet]
        [Route("get-by-id/{itemId}")]
        public async Task<IActionResult> GetItemByIdAsync([FromRoute] Guid itemId)
        {
            var item = await unitOfWork.ItemRepository.GetByIdAsync(itemId);
            if (item == null)
                return NotFound("Item with current identifier does not exist");

            return Ok(item);
        }

        [HttpGet]
        [Route("get-all-by-subcategory/{subcategoryId}")]
        public async Task<IActionResult> GetAllItemsBySubcategoryIdAsync([FromRoute] Guid subcategoryId, int pageNumber, int pageSize)
        {
            var items = await unitOfWork.ItemRepository.GetAllBySubcategoryIdAsync(subcategoryId, pageNumber, pageSize);

            var nextPageItems =
                await unitOfWork.ItemRepository.GetAllBySubcategoryIdAsync(subcategoryId, pageNumber + 1, pageSize);

            bool isNextPageExisted = nextPageItems.Count > 0;

            return Ok(new ItemsResult { Items = items, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("find-items")]
        public async Task<IActionResult> FindItemsAsync(string name, int pageNumber, int pageSize)
        {
            var items = await unitOfWork.ItemRepository.FindByContainedCharactersAsync(name, pageNumber, pageSize);

            var nextPageItems =
                await unitOfWork.ItemRepository.FindByContainedCharactersAsync(name, pageNumber + 1, pageSize);

            bool isNextPageExisted = nextPageItems.Count > 0;

            return Ok(new ItemsResult { Items = items, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("find-items-in-subcategory/{subcategoryId}")]
        public async Task<IActionResult> FindItemsInSubcategoryAsync(Guid subcategoryId, string name, int pageNumber, int pageSize)
        {
            var items = await unitOfWork.ItemRepository.FindByContainedCharactersAsync(subcategoryId, name, pageNumber, pageSize);

            var nextPageItems =
                await unitOfWork.ItemRepository.FindByContainedCharactersAsync(subcategoryId, name, pageNumber + 1, pageSize);

            bool isNextPageExisted = nextPageItems.Count > 0;

            return Ok(new ItemsResult { Items = items, IsNextPageExisted = isNextPageExisted });
        }

        [Authorize]
        [RequestSizeLimit(6 * 2 * 1024 * 1024)]
        [HttpPost]
        [Route("suggest-item")]
        public async Task<IActionResult> AddItemAndReviewOnItAsync([FromBody] AddItemAndReviewOnIt model)
        {
            if (model == null)
                return BadRequest("Request size exceeds the limit");

            model.ReviewText = model.ReviewText.Trim();
            model.ShortReview = Regex.Replace(model.ShortReview.Trim(), @"\s+", " ");
            model.ItemName = Regex.Replace(model.ItemName.Trim(), @"\s+", " ");
            if (model.ItemBrand != null)
                model.ItemBrand = Regex.Replace(model.ItemBrand.Trim(), @"\s+", " ");
            if(string.IsNullOrEmpty(model.ShortReview) || string.IsNullOrEmpty(model.ItemName) || string.IsNullOrEmpty(model.ReviewText))
                return BadRequest();

            if (await unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId) == null)
                return NotFound("Subcategory with current identifier does not exist");

            var items = await unitOfWork.ItemRepository.GetByNameAsync(model.ItemName);
            if (items.Any(x => x.Status == ItemStatus.Verified && x.SubcategoryId == model.SubcategoryId))
                return Conflict("Item with current name already exists");

            if(!imageValidator.IsImage(model.ItemPicture)) return BadRequest("Incorrect picture format");
            foreach (var reviewPictureSource in model.ReviewPictures)
            {
                if(!imageValidator.IsImage(reviewPictureSource)) return BadRequest("Incorrect picture format");
            }

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            try
            {
                var restrictionInfoReply = await restrictionInfoClient.GetRestrictionInfoAsync(
                    new GetRestrictionInfoRequest { UserId = userIdStr });
                if (restrictionInfoReply.RestrictionType == RestrictionType.All || restrictionInfoReply.RestrictionType == RestrictionType.ReviewPosting)
                    return Forbid();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Rpc call threw an exception while trying to reach Restriction microservice");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            try
            {
                await unitOfWork.BeginTransactionAsync();

                var itemToAdd = new Item
                {
                    Id = Guid.NewGuid(), Brand = model.ItemBrand, SubcategoryId = model.SubcategoryId,
                    GeneralEstimation = 0, Name = model.ItemName, ReviewsCount = 0,
                    Status = ItemStatus.Pending, Picture = model.ItemPicture 
                };
                await unitOfWork.ItemRepository.AddAsync(itemToAdd);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ItemCreatedSagaEvent
                {
                    ReviewPictures = model.ReviewPictures, ReviewItemEstimation = model.ReviewItemEstimation,
                    ReviewText = model.ReviewText, ShortReview = model.ShortReview, UserIdCreatedBy = userId,
                    ItemId = itemToAdd.Id
                });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing transaction");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Accepted();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete]
        [Route("remove/{itemId}")]
        public async Task<IActionResult> RemoveItemAsync([FromRoute] Guid itemId)
        {
            var item = await unitOfWork.ItemRepository.GetByIdAsync(itemId);
            if (item == null)
                return NotFound("Item with current identifier does not exist");

            var subcategory = await unitOfWork.SubcategoryRepository.GetByIdAsync(item.SubcategoryId);
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(subcategory!.CategoryId);

            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.ItemRepository.RemoveAsync(itemId);

                subcategory.ReviewsCount -= item.ReviewsCount;
                unitOfWork.SubcategoryRepository.Update(subcategory);

                category!.ReviewsCount -= item.ReviewsCount;
                unitOfWork.CategoryRepository.Update(category);

                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ItemRemovedEvent { ItemId = itemId });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "An exception was thrown while processing item removing method");
                await unitOfWork.RollbackTransactionAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateItemAsync([FromBody] UpdateItemDto model)
        {
            var item = await unitOfWork.ItemRepository.GetByIdAsync(model.Id);
            if (item == null)
                return NotFound("Item with current identifier does not exist");

            if (item.Status != ItemStatus.UnderConsideration)
                return BadRequest("Item must be in Under Consideration status to update it");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            string? oldBrand = item.Brand;
            string oldName = item.Name;

            item.Brand = model.Brand;
            item.Name = model.Name;
            unitOfWork.ItemRepository.Update(item);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "User {UserId} updated item {ItemId} Brand from {OldBrand} to {NewBrand} and Name from {OldName} to {NewName}",
                userIdStr, item.Id, oldBrand, model.Brand, oldName, model.Name);

            return Ok();
        }
    }
}
