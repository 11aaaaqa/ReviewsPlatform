using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Web.MVC.Constants;
using Web.MVC.DTOs.reivew;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.Api_responses.category;
using Web.MVC.Models.Api_responses.review;
using Web.MVC.Models.Api_responses.review.enums;
using Web.MVC.Models.View_models.Category;
using Web.MVC.Models.View_models.Review;
using Web.MVC.Models.View_models.Review.json;
using Web.MVC.Models.View_models.User;
using Web.MVC.Services;

namespace Web.MVC.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ImageConverter imageConverter;
        private readonly ILogger<ReviewController> logger;
        private readonly SortService sortService;
        private readonly string emailUrl;

        public ReviewController(IHttpClientFactory httpClientFactory, ImageConverter imageConverter, ILogger<ReviewController> logger, SortService sortService,
            IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.imageConverter = imageConverter;
            this.logger = logger;
            this.sortService = sortService;
            emailUrl = $"{configuration["CurrentUrl:Scheme"]}://{configuration["CurrentUrl:Domain"]}/settings/email/confirm?token=";
        }

        [Authorize]
        [HttpGet]
        [Route("items/{itemId}/reviews/add")] //sensible AddReviewSelect
        public async Task<IActionResult> AddReview(Guid itemId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var itemResponse = await httpClient.GetAsync($"/api/Item/get-by-id/{itemId}");
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();

            string itemImageSrc = imageConverter.GetImageSrc(item!.Picture);

            var model = new AddReviewViewModel
            {
                Item = item, AddReviewModel = new AddReviewDto(), ItemImageSrc = itemImageSrc,
                ShowEmailConfirmationNotification = false
            };

            if (!User.IsInRole(RoleNames.Verified))
            {
                using StringContent jsonContent = new(JsonSerializer.Serialize(new { Url = emailUrl }), Encoding.UTF8, "application/json");
                var sendTokenResponse = await httpClient.PostAsync("/api/User/send-email-confirmation-token", jsonContent);
                if (!sendTokenResponse.IsSuccessStatusCode && sendTokenResponse.StatusCode != HttpStatusCode.Conflict)
                    sendTokenResponse.EnsureSuccessStatusCode();

                model.ShowEmailConfirmationNotification = true;
            }

            return View(model);
        }

        [Authorize]
        [RequestSizeLimit(5 * 2 * 1024 * 1024)]
        [HttpPost]
        [Route("items/{itemId}/reviews/add")]
        public async Task<IActionResult> AddReview([FromForm] AddReviewDto model)
        {
            if (ModelState.IsValid)
            {
                model.Text = model.Text.Trim();
                model.ShortReview = model.ShortReview.Trim();
                if (string.IsNullOrEmpty(model.ShortReview) || string.IsNullOrEmpty(model.Text) || model.ItemEstimation < 1 || model.ItemEstimation > 5)
                {
                    ModelState.AddModelError(string.Empty, "Заполните все поля");
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                List<byte[]> pictures = new();
                if (model.Pictures.Count > 0)
                {
                    foreach (var picture in model.Pictures)
                    {
                        using MemoryStream memoryStream = new MemoryStream();
                        await picture.CopyToAsync(memoryStream);
                        pictures.Add(memoryStream.ToArray());
                    }
                }

                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new
                { model.ShortReview, model.Text, model.ItemEstimation, model.ItemId, Pictures = pictures }),
                    Encoding.UTF8, "application/json");

                var addReviewResponse = await httpClient.PostAsync("/api/Review/add", jsonContent);
                if (!addReviewResponse.IsSuccessStatusCode)
                {
                    if (addReviewResponse.StatusCode == HttpStatusCode.Forbidden)
                        ModelState.AddModelError(string.Empty, "Вы не можете выполнить данное действие. Узнать причину можно на странице профиля");
                    else if (addReviewResponse.StatusCode == HttpStatusCode.BadRequest)
                        ModelState.AddModelError(string.Empty, "Неверный формат изображения");
                    else if (addReviewResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ModelState.AddModelError(string.Empty, "Что-то пошло не так");
                        logger.LogCritical("Internal server error result was sent while trying to add item with review");
                    }
                    else
                        ModelState.AddModelError(string.Empty, "Что-то пошло не так");

                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                Guid userId = new Guid(userIdStr);
                return RedirectToAction("GetUserById", "User", new { userId }); //сделать редирект на страницу с предложенными отзывами
            }

            return BadRequest(new { errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }

        [Authorize]
        [HttpGet]
        [Route("reviews/add/select")]
        public async Task<IActionResult> AddReviewSelect()
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var categoriesResponse = await httpClient.GetAsync("/api/Category/all");
            categoriesResponse.EnsureSuccessStatusCode();
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            return View(sortService.SortCategories(categories!));
        }

        [Authorize]
        [HttpGet]
        [Route("reviews/add")]
        public async Task<IActionResult> AddReviewWithItem(Guid subcategoryId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var subcategoryResponse = await httpClient.GetAsync($"/api/Subcategory/get-by-id/{subcategoryId}");
            subcategoryResponse.EnsureSuccessStatusCode();

            ViewBag.ShowEmailConfirmationNotification = false;
            if (!User.IsInRole(RoleNames.Verified))
            {
                using StringContent jsonContent = new(JsonSerializer.Serialize(new { Url = emailUrl }), Encoding.UTF8, "application/json");
                var sendTokenResponse = await httpClient.PostAsync("/api/User/send-email-confirmation-token", jsonContent);
                if (!sendTokenResponse.IsSuccessStatusCode && sendTokenResponse.StatusCode != HttpStatusCode.Conflict)
                    sendTokenResponse.EnsureSuccessStatusCode();

                ViewBag.ShowEmailConfirmationNotification = true;
            }

            return View(new AddReviewWithItemDto { SubcategoryId = subcategoryId });
        }

        [Authorize]
        [RequestSizeLimit(6 * 2 * 1024 * 1024)]
        [HttpPost]
        [Route("reviews/add")] //sensible AddReviewWithItem view
        public async Task<IActionResult> AddReviewWithItem([FromForm] AddReviewWithItemDto model)
        {
            if (ModelState.IsValid)
            {
                model.ItemName = model.ItemName.Trim();
                if (model.ItemBrand != null)
                    model.ItemBrand = model.ItemBrand.Trim();
                model.ReviewText = model.ReviewText.Trim();
                model.ShortReview = model.ShortReview.Trim();
                if(string.IsNullOrEmpty(model.ItemName) || string.IsNullOrEmpty(model.ReviewText) || string.IsNullOrEmpty(model.ShortReview)
                   || model.ReviewItemEstimation < 1 || model.ReviewItemEstimation > 5)
                {
                    ModelState.AddModelError(string.Empty, "Заполните все поля");
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                using MemoryStream memoryStream = new MemoryStream();
                await model.ItemPicture.CopyToAsync(memoryStream);
                byte[] itemPicture = memoryStream.ToArray();

                List<byte[]> reviewPictures = new();
                if (model.ReviewPictures.Count > 0)
                {
                    foreach (var reviewPicture in model.ReviewPictures)
                    {
                        using MemoryStream ms = new MemoryStream();
                        await reviewPicture.CopyToAsync(ms);
                        reviewPictures.Add(ms.ToArray());
                    }
                }

                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new
                {
                    model.ItemName, model.ItemBrand, ItemPicture = itemPicture, model.SubcategoryId, model.ShortReview,
                    model.ReviewText, model.ReviewItemEstimation, ReviewPictures = reviewPictures
                }), Encoding.UTF8, "application/json");

                var addReviewWithItemResponse = await httpClient.PostAsync("/api/Item/suggest-item", jsonContent);
                if (!addReviewWithItemResponse.IsSuccessStatusCode)
                {
                    if (addReviewWithItemResponse.StatusCode == HttpStatusCode.Conflict)
                        ModelState.AddModelError(string.Empty, "Товар с таким наименованием уже существует");
                    else if(addReviewWithItemResponse.StatusCode == HttpStatusCode.Forbidden)
                        ModelState.AddModelError(string.Empty, "Вы не можете выполнить данное действие. Узнать причину можно на странице профиля");
                    else if(addReviewWithItemResponse.StatusCode == HttpStatusCode.BadRequest)
                        ModelState.AddModelError(string.Empty, "Неверный формат изображения");
                    else if (addReviewWithItemResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ModelState.AddModelError(string.Empty, "Что-то пошло не так");
                        logger.LogCritical("Internal server error result was sent while trying to add item with review");
                    }
                    else
                        ModelState.AddModelError(string.Empty, "Что-то пошло не так");

                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                Guid userId = new Guid(userIdStr);
                return RedirectToAction("GetUserById", "User", new { userId }); //сделать редирект на страницу с предложенными отзывами
            }

            return BadRequest(new { errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }

        [HttpGet]
        [Route("items/{itemId}/reviews")] //sensible GetItemsBySubcategoryId view
        public async Task<IActionResult> GetReviewsByItemId([FromRoute] Guid itemId, OrderByDate? date, OrderByEstimation? estimation)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var itemResponse = await httpClient.GetAsync($"/api/Item/get-by-id/{itemId}");
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();
            var itemDisplay = new ItemDisplay
            {
                Brand = item.Brand, GeneralEstimation = item.GeneralEstimation, Id = item.Id, Name = item.Name,
                SubcategoryId = item.SubcategoryId, ReviewsCount = item.ReviewsCount,
                PictureSrc = imageConverter.GetImageSrc(item.Picture)
            };

            int pageNumber = 1;
            int pageSize = 30;

            var reviewsResult = await GetReviews(itemId, date, estimation, pageNumber, pageSize);
            if (reviewsResult == null) return BadRequest();

            var reviewsDisplay = await GetReviewDisplayList(reviewsResult.Reviews);

            return View(new GetReviewsByItemIdViewModel
            {
                IsNextPageExisted = reviewsResult.IsNextPageExisted, PageSize = pageSize, Reviews = reviewsDisplay,
                Item = itemDisplay, Date = date, Estimation = estimation
            });
        }

        [Route("items/{itemId}/reviews/json")]
        public async Task<IActionResult> GetReviewsByItemIdJson([FromRoute] Guid itemId, int pageNumber, int pageSize, OrderByDate? date,
            OrderByEstimation? estimation)
        {
            var reviewsResult = await GetReviews(itemId, date, estimation, pageNumber, pageSize);
            if (reviewsResult == null) return BadRequest();

            var reviewsDisplay = await GetReviewDisplayList(reviewsResult.Reviews);

            return new JsonResult(new ReviewDisplayJson
                { IsNextPageExisted = reviewsResult.IsNextPageExisted, Reviews = reviewsDisplay });
        }

        [HttpGet]
        [Route("reviews/{reviewId}")]
        public async Task<IActionResult> GetReviewById([FromRoute] Guid reviewId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var reviewResponse = await httpClient.GetAsync($"/api/Review/get-by-id/{reviewId}");
            reviewResponse.EnsureSuccessStatusCode();
            var review = await reviewResponse.Content.ReadFromJsonAsync<ReviewResponse>();
            
            var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{review!.UserId}");
            userResponse.EnsureSuccessStatusCode();
            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

            List<string> pictures = new List<string>();
            foreach (var picture in review.Pictures)
            {
                pictures.Add(imageConverter.GetImageSrc(picture));
            }

            var userDisplay = new UserDisplay
            {
                AvatarSrc = imageConverter.GetImageSrc(user!.AvatarSource), Email = user.Email, Id = user.Id, Roles = user.Roles,
                IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                RegistrationDate = user.RegistrationDate, UserName = user.UserName
            };

            var itemResponse = await httpClient.GetAsync($"/api/Item/get-by-id/{review.ItemId}");
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();

            var itemDisplay = new ItemDisplay
            {
                Brand = item!.Brand, GeneralEstimation = item.GeneralEstimation, Id = item.Id,
                Name = item.Name, ReviewsCount = item.ReviewsCount, SubcategoryId = item.SubcategoryId,
                PictureSrc = imageConverter.GetImageSrc(item.Picture)
            };

            return View(new ReviewDisplay
            {
                CreatedAt = review.CreatedAt, CreatedByUser = userDisplay, DislikesCount = review.DislikesCount, Id = review.Id,
                Item = itemDisplay, ItemEstimation = review.ItemEstimation, LikesCount = review.LikesCount,
                PicturesSrc = pictures, ShortReview = review.ShortReview, Text = review.Text
            });
        }

        private async Task<ReviewsResultResponse?> GetReviews(Guid itemId, OrderByDate? date, OrderByEstimation? estimation, int pageNumber,
            int pageSize)
        {
            if (date != null && estimation != null) return null;

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            HttpResponseMessage? reviewsResponse;
            if (date != null)
            {
                reviewsResponse = await httpClient.GetAsync(
                    $"/api/Review/get-by-item-id/{itemId}?pageSize={pageSize}&pageNumber={pageNumber}&orderByDate={date}");
            }
            else if (estimation != null)
            {
                reviewsResponse = await httpClient.GetAsync(
                    $"/api/Review/get-by-item-id-by-estimation/{itemId}?pageSize={pageSize}&pageNumber={pageNumber}&orderByEstimation={estimation}");
            }
            else
            {
                reviewsResponse = await httpClient.GetAsync(
                    $"/api/Review/get-by-item-id-by-actuality/{itemId}?pageSize={pageSize}&pageNumber={pageNumber}");
            }

            reviewsResponse.EnsureSuccessStatusCode();
            var reviewsResult = await reviewsResponse.Content.ReadFromJsonAsync<ReviewsResultResponse>();

            return reviewsResult;
        }

        private async Task<List<ReviewNoPictureDisplay>> GetReviewDisplayList(List<ReviewNoPicturesResponse> reviews)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            using StringContent jsonContent = new(JsonSerializer.Serialize(reviews.Select(x => x.UserId).ToList()),
                Encoding.UTF8, "application/json");
            var usersResponse = await httpClient.PostAsync("/api/User/get-users-by-ids", jsonContent);
            usersResponse.EnsureSuccessStatusCode();
            var users = await usersResponse.Content.ReadFromJsonAsync<List<UserResponse>>();

            List<ReviewNoPictureDisplay> reviewsDisplay = new List<ReviewNoPictureDisplay>();
            foreach (var review in reviews)
            {
                var user = users!.Single(x => x.Id == review.UserId);
                var userDisplay = new UserDisplay
                {
                    AvatarSrc = imageConverter.GetImageSrc(user.AvatarSource), Email = user.Email, Id = user.Id,
                    Roles = user.Roles, IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                    RegistrationDate = user.RegistrationDate, UserName = user.UserName
                };

                reviewsDisplay.Add(new ReviewNoPictureDisplay
                {
                    CreatedAt = review.CreatedAt, DislikesCount = review.DislikesCount, Id = review.Id, IsCreatedWithItem = review.IsCreatedWithItem,
                    ItemEstimation = review.ItemEstimation, ItemId = review.ItemId, LikesCount = review.LikesCount,
                    RejectionReason = review.RejectionReason, ReviewStatus = review.ReviewStatus, ShortReview = review.ShortReview,
                    Text = review.Text, User = userDisplay
                });
            }

            return reviewsDisplay;
        }
    }
}
