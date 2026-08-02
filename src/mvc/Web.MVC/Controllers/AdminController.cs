using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.admin;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.Api_responses.category;
using Web.MVC.Models.Api_responses.review;
using Web.MVC.Models.View_models.Admin;
using Web.MVC.Models.View_models.Category;
using Web.MVC.Models.View_models.Review;
using Web.MVC.Models.View_models.User;
using Web.MVC.Services;

namespace Web.MVC.Controllers
{
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ImageConverter imageConverter;

        public AdminController(ILogger<AdminController> logger, IHttpClientFactory httpClientFactory, ImageConverter imageConverter)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.imageConverter = imageConverter;
        }

        [HttpGet]
        [Route("admin/panel")]
        public IActionResult GetAdminPanel()
        {
            return View();
        }

        [HttpGet]
        [Route("admin/panel/users")]
        public async Task<IActionResult> GetUsersList()
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var rolesResponse = await httpClient.GetAsync("/api/Role/all");
            rolesResponse.EnsureSuccessStatusCode();
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();

            return View(new GetUsersListViewModel { Roles = roles!, PageSize = 30 });
        }

        [Route("admin/panel/users/json")]
        public async Task<IActionResult> GetUsersListJson([FromBody] GetUsersListJsonDto model)
        {
            if (model.RoleIds != null && model.RoleIds.Count == 0) model.RoleIds = null;

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                model.SearchQuery, model.RoleIds, model.UserSort, Pagination = new { model.PageSize, model.PageNumber}
            }), Encoding.UTF8, "application/json");

            var usersResponse = await httpClient.PostAsync("/api/User/get-users", jsonContent);
            usersResponse.EnsureSuccessStatusCode();
            var usersResult = await usersResponse.Content.ReadFromJsonAsync<UsersResultResponse>();

            UsersResultDisplayModel usersResultDisplayModel = new()
            {
                IsNextPageExisted = usersResult!.IsNextPageExisted,
                TotalUsersCount = usersResult.TotalUsersCount, Users = new List<UserFormattedDateDisplay>()
            };

            foreach (var user in usersResult.Users)
            {
                usersResultDisplayModel.Users.Add(new UserFormattedDateDisplay
                {
                    AvatarSource = user.AvatarSource, Email = user.Email, Id = user.Id, Roles = user.Roles,
                    IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified, UserName = user.UserName,
                    RegistrationDate = user.RegistrationDate.ToString(CultureInfo.GetCultureInfo("ru-RU"))
                });
            }

            return new JsonResult(usersResultDisplayModel);
        }

        [HttpGet]
        [Route("admin/panel/under-consideration/reviews")]
        public async Task<IActionResult> GetReviews(int pageNumber = 1)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            int pageSize = 30;
            var reviewsResponse = await httpClient.GetAsync(
                $"/api/Review/get-under-consideration?pageNumber={pageNumber}&pageSize={pageSize}");
            reviewsResponse.EnsureSuccessStatusCode();
            var reviewsResult = await reviewsResponse.Content.ReadFromJsonAsync<ReviewsResultResponse>();

            return View(new GetReviewsViewModel
            {
                CurrentPageNumber = pageNumber, Reviews = reviewsResult!.Reviews,
                IsNextPageExisted = reviewsResult.IsNextPageExisted
            });
        }

        [HttpGet]
        [Route("admin/panel/under-consideration/reviews/{reviewId}")]
        public async Task<IActionResult> GetReviewById([FromRoute] Guid reviewId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var reviewResponse = await httpClient.GetAsync($"/api/Review/get-by-id/{reviewId}");
            reviewResponse.EnsureSuccessStatusCode();
            var review = await reviewResponse.Content.ReadFromJsonAsync<ReviewResponse>();

            var itemResponse = await httpClient.GetAsync($"/api/Item/get-by-id/{review!.ItemId}");
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();

            var itemDisplay = new ItemDisplay
            {
                Id = item!.Id, Brand = item.Brand, GeneralEstimation = item.GeneralEstimation,
                Name = item.Name, ReviewsCount = item.ReviewsCount, SubcategoryId = item.SubcategoryId,
                PictureSrc = imageConverter.GetImageSrc(item.Picture)
            };

            var userCreatedByResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{review.UserId}");
            userCreatedByResponse.EnsureSuccessStatusCode();
            var userCreatedBy = await userCreatedByResponse.Content.ReadFromJsonAsync<UserResponse>();

            var userCreatedByDisplay = new UserDisplay
            {
                Id = userCreatedBy!.Id, Roles = userCreatedBy.Roles, Email = userCreatedBy.Email,
                IsAvatarDefault = userCreatedBy.IsAvatarDefault, IsEmailVerified = userCreatedBy.IsEmailVerified,
                RegistrationDate = userCreatedBy.RegistrationDate, UserName = userCreatedBy.UserName,
                AvatarSrc = imageConverter.GetImageSrc(userCreatedBy.AvatarSource)
            };

            ReviewDisplay reviewDisplay = new ReviewDisplay
            {
                CommentsCount = review.CommentsCount, CreatedAt = review.CreatedAt,
                DislikesCount = review.DislikesCount,
                Id = review.Id, ItemEstimation = review.ItemEstimation, LikesCount = review.LikesCount,
                ShortReview = review.ShortReview, Text = review.Text, IsCreatedWithItem = review.IsCreatedWithItem,
                Item = itemDisplay, CreatedByUser = userCreatedByDisplay
            };

            foreach (var picture in review.Pictures)
            {
                reviewDisplay.PicturesSrc.Add(imageConverter.GetImageSrc(picture));
            }

            GetReviewByIdAdminViewModel model = new() { Review = reviewDisplay, IsUserRestricted = false };

            var restrictionResponse = await httpClient.GetAsync($"/api/Restriction/get-active/{userCreatedBy.Id}");
            if (restrictionResponse.IsSuccessStatusCode)
            {
                model.IsUserRestricted = true;
            }
            else
            {
                if (restrictionResponse.StatusCode != HttpStatusCode.NotFound)
                    restrictionResponse.EnsureSuccessStatusCode();
            }

            if (review.IsCreatedWithItem)
            {
                var subcategoryResponse = await httpClient.GetAsync($"/api/Subcategory/get-by-id/{item.SubcategoryId}");
                subcategoryResponse.EnsureSuccessStatusCode();
                var subcategory = await subcategoryResponse.Content.ReadFromJsonAsync<SubcategoryResponse>();

                var categoryResponse = await httpClient.GetAsync($"/api/Category/get-by-id/{subcategory!.CategoryId}");
                categoryResponse.EnsureSuccessStatusCode();
                var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
                
                model.CategoryInfo = new GetReviewCategoryInfo(CategoryName: category!.Name,
                    SubcategoryId: subcategory.Id, SubcategoryName: subcategory.Name);
            }

            return View(model);
        }

        [HttpPost]
        [Route("admin/panel/under-consideration/reviews/{reviewId}/accept")]
        public async Task<IActionResult> AcceptReview([FromRoute] Guid reviewId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var acceptReviewResponse = await httpClient.GetAsync($"/api/Review/accept-review/{reviewId}");
            acceptReviewResponse.EnsureSuccessStatusCode();

            return RedirectToAction("GetReviews");
        }

        [HttpPost]
        [Route("admin/panel/under-consideration/reviews/{reviewId}/reject")]
        public async Task<IActionResult> RejectReview([FromRoute] Guid reviewId, [FromForm] RejectReviewDto model)
        {
            if (model.AddRestriction is { IsPermanent: false, DurationInDays: <= 0})
                ModelState.AddModelError(string.Empty, "Длительность блокировки не может быть отрицательной или равной 0");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
            
            if (model.AddRestriction != null)
            {
                TimeSpan duration = TimeSpan.Zero;
                if (!model.AddRestriction.IsPermanent)
                    duration = new TimeSpan(days: model.AddRestriction.DurationInDays, 0, 0, 0);
                
                using StringContent jsonContent = new(JsonSerializer.Serialize(new
                {
                    ReviewId = reviewId,
                    model.RejectionReason,
                    AddRestriction = new
                    {
                        model.AddRestriction.RestrictionType,
                        model.AddRestriction.IsPermanent,
                        model.AddRestriction.Reason,
                        Duration = duration
                    }
                }), Encoding.UTF8, "application/json");

                var rejectReviewResponse = await httpClient.PutAsync("/api/Review/reject-review", jsonContent);
                rejectReviewResponse.EnsureSuccessStatusCode();
            }
            else
            {
                using StringContent jsonContent = new(JsonSerializer.Serialize(new 
                    { ReviewId = reviewId, model.RejectionReason }), Encoding.UTF8, "application/json");

                var rejectReviewResponse = await httpClient.PutAsync("/api/Review/reject-review", jsonContent);
                rejectReviewResponse.EnsureSuccessStatusCode();
            }

            return RedirectToAction("GetReviews");
        }
    }
}
