using CategoryMicroservice.Api.Constants;
using CategoryMicroservice.Api.DTOs.Subcategory;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Messages.Category;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CategoryMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController(IUnitOfWork unitOfWork, ILogger<SubcategoryController> logger, IMessagePublisher messagePublisher)
        : ControllerBase
    {
        [HttpGet]
        [Route("get-by-name")]
        public async Task<IActionResult> GetSubcategoryByNameAsync(string name)
        {
            var subcategory = await unitOfWork.SubcategoryRepository.FindByNameAsync(name);
            if (subcategory == null)
                return NotFound("Subcategory with current name does not exist");

            return Ok(subcategory);
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllSubcategoriesAsync()
            => Ok(await unitOfWork.SubcategoryRepository.GetAllAsync());

        [HttpGet]
        [Route("find-by-contained-characters")]
        public async Task<IActionResult> GetSubcategoriesByContainedCharactersAsync(string name)
            => Ok(await unitOfWork.SubcategoryRepository.FindByContainedCharactersInNameAsync(name));

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddSubcategoryAsync([FromBody] AddSubcategoryDto model)
        {
            model.Name = Regex.Replace(model.Name.Trim(), @"\s+", " ");

            var subcategory = await unitOfWork.SubcategoryRepository.FindByNameAsync(model.Name);
            if (subcategory != null)
                return Conflict("Subcategory with current name already exists");

            if (await unitOfWork.CategoryRepository.GetByIdAsync(model.CategoryId) == null)
                return BadRequest("Category with current identifier does not exist");

            var subcategoryToAdd = new Subcategory { Id = Guid.NewGuid(), Name = model.Name, CategoryId = model.CategoryId, ReviewsCount = 0};
            await unitOfWork.SubcategoryRepository.AddAsync(subcategoryToAdd);
            await unitOfWork.CompleteAsync();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} created new subcategory {SubcategoryId} of category {CategoryId}",
                userIdStr, subcategoryToAdd.Id, subcategoryToAdd.CategoryId);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateSubcategoryNameAsync([FromBody] UpdateSubcategoryNameDto model)
        {
            var subcategory = await unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcategory == null)
                return NotFound("Subcategory with current identifier does not exist");

            if (await unitOfWork.SubcategoryRepository.FindByNameAsync(model.NewName) != null)
                return Conflict("Subcategory with current name already exists");

            string oldSubcategoryName = subcategory.Name;

            subcategory.Name = model.NewName;
            unitOfWork.SubcategoryRepository.Update(subcategory);
            await unitOfWork.CompleteAsync();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} updated subcategory {SubcategoryId} name from {OldSubcategoryName} to {NewSubcategoryName}",
                userIdStr, subcategory.Id, oldSubcategoryName, subcategory.Name);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete]
        [Route("remove/{subcategoryId}")]
        public async Task<IActionResult> RemoveSubcategoryAsync([FromRoute] Guid subcategoryId)
        {
            var subcategory = await unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId);
            if (subcategory == null)
                return NotFound("Subcategory with current identifier does not exist");

            var category = await unitOfWork.CategoryRepository.GetByIdAsync(subcategory.CategoryId);

            string subcategoryName = subcategory.Name;
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await unitOfWork.BeginTransactionAsync();

                category!.ReviewsCount -= subcategory.ReviewsCount;
                unitOfWork.CategoryRepository.Update(category);

                await unitOfWork.SubcategoryRepository.RemoveAsync(subcategoryId);

                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new SubcategoryRemovedEvent { SubcategoryId = subcategoryId });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "User {UserId} tried to remove subcategory {SubcategoryName} but transaction threw an exception",
                    userIdStr, subcategoryName);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            logger.LogInformation("User {UserId} removed subcategory {SubcategoryName}", userIdStr, subcategoryName);

            return Ok();
        }
    }
}
