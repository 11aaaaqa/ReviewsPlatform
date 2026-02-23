using System.Net;
using System.Security.Claims;
using CategoryMicroservice.Api.Constants;
using CategoryMicroservice.Api.DTOs.Category;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;
using CategoryMicroservice.Api.Services.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryRepository<Category> categoryRepository, IUnitOfWork unitOfWork, 
        ILogger<CategoryController> logger) : ControllerBase
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
        {
            var categories = await categoryRepository.FindByContainedCharactersInNameAsync(name);
            if (categories.Count == 0)
                return NotFound();

            return Ok(categories);
        }

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
            logger.LogInformation("{Timestamp}: User {UserId} created new category with Identifier {CategoryId} and Name {CategoryName}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), currentUserIdStr, categoryToAdd.Id, categoryToAdd.Name);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateCategoryNameAsync([FromBody] UpdateCategoryNameDto model)
        {
            var category = await categoryRepository.GetByIdAsync(model.Id);
            if(category == null) return NotFound("Category with current identifier does not exist");

            string oldCategoryName = category.Name;

            category.Name = model.Name;
            await unitOfWork.CategoryRepository.UpdateAsync(category);
            await unitOfWork.CompleteAsync();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("{Timestamp}: User {UserId} updated category {CategoryId} name from {OldCategoryName} to {NewCategoryName}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userIdStr, category.Id, oldCategoryName, model.Name);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete]
        [Route("{categoryId}/remove")]
        public async Task<IActionResult> RemoveCategoryAsync([FromRoute] Guid categoryId)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.CategoryRepository.RemoveAsync(categoryId);
                await unitOfWork.CompleteAsync();

                //rabbitmq publish

                await unitOfWork.CommitTransactionAsync();
            }
            catch (ArgumentException)
            {
                return NotFound("Category with current identifier does not exist");
            }
            catch (Exception exc)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical("{Timestamp}: Admin {UserId} tried to remove category {CategoryId} but transaction threw an error: {ErrorMessage}",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userIdStr, categoryId, exc.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            logger.LogInformation("{Timestamp}: User {UserId} removed Category {CategoryId}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userIdStr, categoryId);

            return Ok();
        }
    }
}
