using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.user;
using Web.MVC.Filters.ActionFilters;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.View_models.User;
using Web.MVC.Services;

namespace Web.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string url;
        private readonly ILogger<UserController> logger;
        private readonly AvatarConverter avatarConverter;
        private readonly List<string> availableFileExtensions = new() { ".jpg", ".png", ".jpeg"};
        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<UserController> logger,
            AvatarConverter avatarConverter)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.avatarConverter = avatarConverter;
            url = $"{configuration["ApiGateway:Protocol"]}://{configuration["ApiGateway:Domain"]}";
        }

        [Route("users/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            var userResponse = await httpClient.GetAsync($"{url}/api/User/get-user-by-id/{userId}");
            userResponse.EnsureSuccessStatusCode();
            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

            bool canUserSetTheRoles = User.IsInRole(RoleNames.Admin);
            bool canUserViewTheRoles = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Moderator);
            string avatarSrc = avatarConverter.GetAvatarSrc(user.AvatarSource);
            var model = new GetUserByIdViewModel
            {
                User = user, CanUserSetTheRoles = canUserSetTheRoles,
                CanUserViewTheRoles = canUserViewTheRoles, AvatarSrc = avatarSrc
            };
            if (canUserSetTheRoles)
            {
                var allRolesResponse = await httpClient.GetAsync($"{url}/api/Role/all");
                allRolesResponse.EnsureSuccessStatusCode();
                var allRoles = await allRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
                model.AllRoles = allRoles;
            }

            return View(model);
        }

        [Authorize(Roles = RoleNames.Admin)]
        [Route("users/{userId}/set-roles")]
        [HttpPost]
        public async Task<IActionResult> SetUserRoles(Guid userId, List<Guid> roleIds, string returnUrl)
        {
            using StringContent jsonContent = new(JsonSerializer.Serialize(new { UserId = userId, RoleIds = roleIds }),
                Encoding.UTF8, "application/json");
            HttpClient httpClient = httpClientFactory.CreateClient();

            var setUserRolesResponse = await httpClient.PostAsync($"{url}/api/User/set-user-roles", jsonContent);
            setUserRolesResponse.EnsureSuccessStatusCode();

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/settings")]
        [HttpGet]
        public async Task<IActionResult> EditUserProfile(Guid userId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            var userResponse = await httpClient.GetAsync($"{url}/api/User/get-user-by-id/{userId}");
            if (!userResponse.IsSuccessStatusCode)
            {
                if (userResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogCritical("User, with Id {UserId} that has been extracted from user's claim, is not in Users database", userId);
                    return StatusCode((int)HttpStatusCode.NotFound);
                }
                logger.LogCritical("Something went wrong while trying to get user with Id {UserId}", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

            string avatarSrc = avatarConverter.GetAvatarSrc(user!.AvatarSource);

            return View(new EditUserProfileViewModel {AvatarSrc = avatarSrc, UserEmail = user.Email, UserId = userId});
        }

        [RequestSizeLimit(2 * 1024 * 1024)] // При изменении изменить в EditUserProfile view "Размер файла превышает 2 мб" на актуальное значение
        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/update-avatar")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserAvatar(UpdateAvatarDto model, Guid userId, string returnUrl)
        {
            using MemoryStream memoryStream = new MemoryStream();
            await model.Image.CopyToAsync(memoryStream);
            if (memoryStream.Length > 2 * 1024 * 1024)
                return StatusCode((int)HttpStatusCode.BadRequest, "Размер файла превышает 2 мб");
            
            if (!availableFileExtensions.Any(x => x == Path.GetExtension(model.Image.FileName)))
                return StatusCode((int)HttpStatusCode.BadRequest, "Неверный формат");
            
            byte[] avatarSource = memoryStream.ToArray();
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            { UserId = userId, AvatarSource = avatarSource }), Encoding.UTF8, "application/json");
            HttpClient httpClient = httpClientFactory.CreateClient();

            var response = await httpClient.PutAsync($"{url}/api/User/set-avatar", jsonContent);
            response.EnsureSuccessStatusCode();

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction("EditUserProfile", routeValues: new { userId });

            return LocalRedirect(returnUrl);
        }
    }
}
