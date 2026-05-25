using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.category;
using Web.MVC.Models.Api_responses.category;
using Web.MVC.Models.View_models.Category;
using Web.MVC.Services;

namespace Web.MVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<CategoryController> logger;
        private readonly ImageConverter imageConverter;
        private readonly SortService sortService;
        public CategoryController(IHttpClientFactory httpClientFactory, ILogger<CategoryController> logger, ImageConverter imageConverter, SortService sortService)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.imageConverter = imageConverter;
            this.sortService = sortService;
        }

        [HttpGet]
        [Route("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var categoriesResponse = await httpClient.GetAsync("/api/Category/all");
            categoriesResponse.EnsureSuccessStatusCode();
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            categories = sortService.SortCategories(categories!);

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
            categoriesResponse.EnsureSuccessStatusCode();
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            categories = sortService.SortCategories(categories!);

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

        [HttpGet]
        [Route("categories/{subcategoryId}")]
        public async Task<IActionResult> GetItemsBySubcategoryId([FromRoute] Guid subcategoryId, string? query)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var subcategoryResponse= await httpClient.GetAsync($"/api/Subcategory/get-by-id/{subcategoryId}");
            subcategoryResponse.EnsureSuccessStatusCode();
            var subcategory = await subcategoryResponse.Content.ReadFromJsonAsync<SubcategoryResponse>();

            int pageSize = 10; //изменить на 30
            int pageNumber = 1;

            ItemsResult itemsResult;

            if (query == null)
            {
                var itemsResponse = await httpClient.GetAsync(
                    $"/api/Item/get-all-by-subcategory/{subcategoryId}?pageNumber={pageNumber}&pageSize={pageSize}");
                itemsResponse.EnsureSuccessStatusCode();
                itemsResult = await itemsResponse.Content.ReadFromJsonAsync<ItemsResult>();
            }
            else
            {
                query = HttpUtility.UrlEncode(query);
                var itemsResponse = await httpClient.GetAsync(
                    $"/api/Item/find-items-in-subcategory/{subcategoryId}?name={query}&pageNumber={pageNumber}&pageSize={pageSize}");
                itemsResponse.EnsureSuccessStatusCode();
                itemsResult = await itemsResponse.Content.ReadFromJsonAsync<ItemsResult>();
            }

            List<ItemDisplay> items = new List<ItemDisplay>();
            foreach (var item in itemsResult.Items)
            {
                items.Add(new ItemDisplay
                {
                    Id = item.Id, Brand = item.Brand, GeneralEstimation = item.GeneralEstimation,
                    Name = item.Name, SubcategoryId = item.SubcategoryId, ReviewsCount = item.ReviewsCount,
                    PictureSrc = imageConverter.GetImageSrc(item.Picture)
                });
            }

            return View(new GetItemsBySubcategoryIdViewModel
            {
                Subcategory = subcategory!, Items = items, IsNextPageExisted = itemsResult.IsNextPageExisted, PageSize = pageSize
            });
        }

        [Route("categories/{subcategoryId}/json")] //sensitive GetItemsBySubcategoryId
        public async Task<IActionResult> GetItemsBySubcategoryIdJson([FromRoute] Guid subcategoryId, string? query, int pageNumber, int pageSize)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            ItemsResult itemsResult;

            if (query == null)
            {
                var itemsResponse = await httpClient.GetAsync(
                    $"/api/Item/get-all-by-subcategory/{subcategoryId}?pageNumber={pageNumber}&pageSize={pageSize}");
                itemsResponse.EnsureSuccessStatusCode();
                itemsResult = await itemsResponse.Content.ReadFromJsonAsync<ItemsResult>();
            }
            else
            {
                query = HttpUtility.UrlEncode(query);
                var itemsResponse = await httpClient.GetAsync(
                    $"/api/Item/find-items-in-subcategory/{subcategoryId}?name={query}&pageNumber={pageNumber}&pageSize={pageSize}");
                itemsResponse.EnsureSuccessStatusCode();
                itemsResult = await itemsResponse.Content.ReadFromJsonAsync<ItemsResult>();
            }

            List<ItemDisplay> items = new List<ItemDisplay>();
            foreach (var item in itemsResult.Items)
            {
                items.Add(new ItemDisplay
                {
                    Id = item.Id, Brand = item.Brand, GeneralEstimation = item.GeneralEstimation, Name = item.Name,
                    SubcategoryId = item.SubcategoryId, ReviewsCount = item.ReviewsCount, PictureSrc = imageConverter.GetImageSrc(item.Picture)
                });
            }

            return new JsonResult(new { items, itemsResult.IsNextPageExisted });
        }

        [Route("items/json/search")] //sensitive AddReviewSelect view
        public async Task<IActionResult> FindItemsJson(string query, int pageNumber, int pageSize)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            query = HttpUtility.UrlEncode(query);

            var findItemsResponse = await httpClient.GetAsync(
                $"/api/Item/find-items?name={query}&pageNumber={pageNumber}&pageSize={pageSize}");
            findItemsResponse.EnsureSuccessStatusCode();
            var itemsResult = await findItemsResponse.Content.ReadFromJsonAsync<ItemsResult>();

            var model = new FindItemsJsonViewModel { IsNextPageExisted = itemsResult!.IsNextPageExisted, Items = new List<ItemDisplay>() };
            foreach (var itemResponse in itemsResult.Items)
            {
                model.Items.Add(new ItemDisplay
                {
                    Brand = itemResponse.Brand, GeneralEstimation = double.Round(itemResponse.GeneralEstimation, 1), Id = itemResponse.Id,
                    Name = itemResponse.Name, SubcategoryId = itemResponse.SubcategoryId, ReviewsCount = itemResponse.ReviewsCount,
                    PictureSrc = imageConverter.GetImageSrc(itemResponse.Picture)
                });
            }

            return new JsonResult(model);
        }
    }
}
