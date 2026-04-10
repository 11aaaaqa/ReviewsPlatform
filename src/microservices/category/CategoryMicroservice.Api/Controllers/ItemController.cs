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
using MessageBus.Messages.Item;
using MessageBus.Messages.Saga.CreateItemWIthReview;

namespace CategoryMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher, ILogger<ItemController> logger) : ControllerBase
    {
        private readonly List<string> availablePictureExtensions = new() { ".jpg", ".png", ".jpeg" };

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

            if (model.ReviewPictures.Count > 5) 
                return BadRequest("Review's pictures count exceeds the count of 5");

            if (await unitOfWork.ItemRepository.GetByNameAsync(model.ItemName) != null)
                return Conflict("Item with current name already exists");

            if (await unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId) == null)
                return NotFound("Subcategory with current identifier does not exist");

            //gRPC to check if user allowed to add

            bool isItemPictureValid = availablePictureExtensions.Any(x => x == Path.GetExtension(model.ItemPicture.FileName).ToLower());
            bool isReviewPicturesValid = true;
            if (model.ReviewPictures.Count > 0)
                isReviewPicturesValid = model.ReviewPictures.All(x => availablePictureExtensions.Contains(Path.GetExtension(x.FileName).ToLower()));

            if (!isItemPictureValid || !isReviewPicturesValid) return BadRequest("Incorrect picture format");

            using MemoryStream memoryStream = new MemoryStream();
            await model.ItemPicture.CopyToAsync(memoryStream);
            byte[] itemPicture = memoryStream.ToArray();

            List<byte[]> reviewsPictures = new();
            foreach (var reviewPicture in model.ReviewPictures)
            {
                await reviewPicture.CopyToAsync(memoryStream);
                reviewsPictures.Add(memoryStream.ToArray());
            }

            model.ShortReview = Regex.Replace(model.ShortReview.Trim(), @"\s+", " ");
            model.ItemName = Regex.Replace(model.ItemName.Trim(), @"\s+", " ");
            if(model.ItemBrand != null)
                model.ItemBrand = Regex.Replace(model.ItemBrand.Trim(), @"\s+", " ");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            try
            {
                await unitOfWork.BeginTransactionAsync();

                var itemToAdd = new Item
                {
                    Id = Guid.NewGuid(), Brand = model.ItemBrand, SubcategoryId = model.SubcategoryId,
                    GeneralEstimation = 0, Name = model.ItemName, ReviewsCount = 0,
                    Status = ItemStatus.Pending, Picture = itemPicture 
                };
                await unitOfWork.ItemRepository.AddAsync(itemToAdd);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ItemCreatedSagaEvent
                {
                    ReviewPictures = reviewsPictures, ReviewItemEstimation = model.ReviewItemEstimation,
                    ReviewText = model.ReviewText, ShortReview = model.ShortReview, UserIdCreatedBy = userId,
                    ItemId = itemToAdd.Id
                });

                await unitOfWork.CompleteAsync();
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
            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.ItemRepository.RemoveAsync(itemId);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ItemRemovedEvent { ItemId = itemId });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (ArgumentException)
            {
                await unitOfWork.RollbackTransactionAsync();
                return NotFound("Item with current identifier does not found");
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
