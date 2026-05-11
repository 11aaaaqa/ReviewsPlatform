using System.Net;
using System.Security.Claims;
using CategoryMicroservice.Api.Constants;
using CategoryMicroservice.Api.DTOs.Category;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Messages.Category;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryRepository<Category> categoryRepository, IUnitOfWork unitOfWork, 
        ILogger<CategoryController> logger, IMessagePublisher messagePublisher) : ControllerBase
    {
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllCategoriesAsync()
            => Ok(await categoryRepository.GetAllAsync());

        [HttpGet]
        [Route("find-by-name")]
        public async Task<IActionResult> GetCategoryByNameAsync(string categoryName)
        {
            var category = await categoryRepository.FindByNameAsync(categoryName);
            if (category == null)
                return NotFound("Category not found");

            return Ok(category);
        }

        [HttpGet]
        [Route("find-by-contained-characters")]
        public async Task<IActionResult> GetCategoriesByContainedCharactersAsync(string name)
            => Ok(await categoryRepository.FindByContainedCharactersInNameAsync(name));

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddCategoryAsync([FromBody] AddCategoryDto model)
        {
            var existingCategory = await categoryRepository.FindByNameAsync(model.Name);
            if (existingCategory != null)
                return Conflict("Category with current name already exists");

            var categoryToAdd = new Category { Id = Guid.NewGuid(), Name = model.Name, ReviewsCount = 0 };
            await unitOfWork.CategoryRepository.AddAsync(categoryToAdd);
            await unitOfWork.CompleteAsync();

            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} created new category with identifier {CategoryId} and Name {CategoryName}",
                currentUserIdStr, categoryToAdd.Id, categoryToAdd.Name);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateCategoryNameAsync([FromBody] UpdateCategoryNameDto model)
        {
            var category = await categoryRepository.GetByIdAsync(model.Id);
            if(category == null) return NotFound("Category with current identifier does not exist");

            if (await unitOfWork.CategoryRepository.FindByNameAsync(model.Name) != null)
                return Conflict("Category with current name already exists");

            string oldCategoryName = category.Name;

            category.Name = model.Name;
            unitOfWork.CategoryRepository.Update(category);
            await unitOfWork.CompleteAsync();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} updated category {CategoryId} name from {OldCategoryName} to {NewCategoryName}",
                userIdStr, category.Id, oldCategoryName, model.Name);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete]
        [Route("{categoryId}/remove")]
        public async Task<IActionResult> RemoveCategoryAsync([FromRoute] Guid categoryId)
        {
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if(category == null)
                return NotFound("Category with current identifier does not exist");
            
            if (category.ReviewsCount > 0)
                return BadRequest("Category cannot be removed until it has at least 1 review on it");

            List<Guid> subcategoryIds = category.Subcategories.Select(x => x.Id).ToList();
            List<Item> itemsToDelete = await unitOfWork.ItemRepository.GetAllBySubcategoryIdAsync(subcategoryIds);
            List<Guid> itemIdsToDelete = itemsToDelete.Select(x => x.Id).ToList();

            string categoryName = category.Name;
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.CategoryRepository.RemoveAsync(categoryId);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new CategoryRemovedEvent
                    { CategoryId = categoryId, ItemIdsOfRemovedCategory = itemIdsToDelete });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception exc)
            {
                await unitOfWork.RollbackTransactionAsync();

                logger.LogCritical(exc, "User {UserId} tried to remove category {CategoryName} but transaction threw an exception", 
                    userIdStr, categoryName);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            logger.LogInformation("User {UserId} removed category {CategoryName}", userIdStr, categoryName);

            return Ok();
        }
    }
}
