using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.category;
using Web.MVC.Models.Api_responses.category;
using Web.MVC.Models.View_models.Category;

namespace Web.MVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<CategoryController> logger;
        public CategoryController(IHttpClientFactory httpClientFactory, ILogger<CategoryController> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        [HttpGet]
        [Route("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var categoriesResponse = await httpClient.GetAsync("/api/Category/all");
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            categories = SortCategories(categories!);

            bool isUserAllowedToAddCategory = User.Identity.IsAuthenticated && User.IsInRole(RoleNames.Admin);

            return View(new GetAllCategoriesViewModel { Categories = categories, IsUserAllowedToAddCategory = isUserAllowedToAddCategory });
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet]
        [Route("categories/edit")]
        public async Task<IActionResult> EditCategories()
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var categoriesResponse = await httpClient.GetAsync("/api/Category/all");
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            categories = SortCategories(categories!);

            return View(categories);
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("categories/{categoryId}/remove")]
        public async Task<IActionResult> RemoveCategory([FromRoute] Guid categoryId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var removeResponse = await httpClient.DeleteAsync($"/api/Category/{categoryId}/remove");
            removeResponse.EnsureSuccessStatusCode();

            return RedirectToAction("EditCategories");
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("subcategories/{subcategoryId}/remove")]
        public async Task<IActionResult> RemoveSubcategory([FromRoute] Guid subcategoryId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var removeResponse = await httpClient.DeleteAsync($"/api/Subcategory/remove/{subcategoryId}");
            removeResponse.EnsureSuccessStatusCode();

            return RedirectToAction("EditCategories");
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("categories/add")]
        public async Task<IActionResult> AddCategory(AddCategoryDto model)
        {
            if (ModelState.IsValid)
            {
                model.Name = model.Name.Trim();
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                var addResponse = await httpClient.PostAsync("/api/Category/add", jsonContent);
                if (addResponse.StatusCode == HttpStatusCode.Conflict)
                    return Conflict("Категория уже существует");

                addResponse.EnsureSuccessStatusCode();

                return RedirectToAction("EditCategories");
            }

            if (string.IsNullOrEmpty(model.Name))
                return BadRequest("Заполните все поля");
            if (model.Name.Trim().Length > StringLengthDtoConstants.CategoryNameMax)
                return BadRequest($"Максимальное количество символов - {StringLengthDtoConstants.CategoryNameMax}");

            return BadRequest("Неверный формат");
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("categories/subcategories/add")]
        public async Task<IActionResult> AddSubcategory(AddSubcategoryDto model)
        {
            if (model.CategoryId == Guid.Empty)
                return BadRequest("Заполните все поля");

            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                var addResponse = await httpClient.PostAsync("/api/Subcategory/add", jsonContent);
                if (addResponse.StatusCode == HttpStatusCode.Conflict)
                    return Conflict("Такая подкатегория уже существует");
                if (addResponse.StatusCode == HttpStatusCode.BadRequest)
                    return BadRequest("Такой категории не существует");

                addResponse.EnsureSuccessStatusCode();

                return RedirectToAction("EditCategories");
            }

            if (string.IsNullOrEmpty(model.Name))
                return BadRequest("Заполните все поля");
            if (model.Name.Trim().Length > StringLengthDtoConstants.SubcategoryNameMax)
                return BadRequest($"Максимальное количество символов - {StringLengthDtoConstants.SubcategoryNameMax}");

            return BadRequest("Неверный формат");
        }

        private List<CategoryResponse> SortCategories(List<CategoryResponse> categories)
        {
            categories = categories
                .OrderBy(x => x.Name.ToLower().Contains("разное") || x.Name.ToLower().Contains("другое"))
                .ThenBy(x => x.ReviewsCount)
                .Select(x => new CategoryResponse
                {
                    Name = x.Name,
                    Id = x.Id,
                    ReviewsCount = x.ReviewsCount,
                    Subcategories = x.Subcategories
                        .OrderBy(c => c.Name.ToLower().Contains("разное") || c.Name.ToLower().Contains("другое"))
                        .ThenBy(c => c.ReviewsCount).ToList()
                }).ToList();
            return categories;
        }
    }
}
