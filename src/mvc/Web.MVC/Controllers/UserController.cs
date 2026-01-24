using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
        private readonly ILogger<UserController> logger;
        private readonly AvatarConverter avatarConverter;
        private readonly IDataProtector dataProtector;
        private readonly List<string> availableFileExtensions = new() { ".jpg", ".png", ".jpeg"};
        public UserController(IHttpClientFactory httpClientFactory, ILogger<UserController> logger,
            AvatarConverter avatarConverter, IDataProtectionProvider dataProtectionProvider)
        {
            dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposeConstants.Jwt);
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.avatarConverter = avatarConverter;
        }

        [Route("users/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{userId}");
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
                var allRolesResponse = await httpClient.GetAsync("/api/Role/all");
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
            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid currentUserId = new Guid(currentUserIdStr);
            if (currentUserId == userId)
            {
                logger.LogWarning("{Timestamp}: User {UserId} tried to set roles to himself",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), currentUserId);
                return BadRequest();
            }

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var allRolesResponse = await httpClient.GetAsync("/api/Role/all");
            allRolesResponse.EnsureSuccessStatusCode();
            var allRoles = await allRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();

            using StringContent jsonContent = new(JsonSerializer.Serialize(new { UserId = userId, RoleIds = roleIds }),
                Encoding.UTF8, "application/json");

            var setUserRolesResponse = await httpClient.PostAsync("/api/User/set-user-roles", jsonContent);
            setUserRolesResponse.EnsureSuccessStatusCode();

            List<string> assignedRoleNames = allRoles!.Where(x => roleIds.Contains(x.Id)).Select(x => x.Name).ToList();
            logger.LogInformation("{Timestamp}: User {AdminId} set roles to user {UserId}: {Roles}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), currentUserId, userId, assignedRoleNames);

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/settings")]
        [HttpGet]
        public async Task<IActionResult> EditUserProfile(Guid userId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{userId}");
            if (!userResponse.IsSuccessStatusCode)
            {
                if (userResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogCritical("{Timestamp}: User, with Id {UserId} that has been extracted from user's claim, is not in Users database",
                        DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId);
                    return StatusCode((int)HttpStatusCode.NotFound);
                }
                logger.LogCritical("{Timestamp}: Something went wrong while trying to get user with Id {UserId}",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

            string avatarSrc = avatarConverter.GetAvatarSrc(user!.AvatarSource);

            return View(new EditUserProfileViewModel
            {
                AvatarSrc = avatarSrc, UserEmail = user.Email, UserId = userId, IsAvatarDefault = user.IsAvatarDefault,
                UserName = user.UserName
            });
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
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var response = await httpClient.PutAsync("/api/User/set-avatar", jsonContent);
            response.EnsureSuccessStatusCode();

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction("EditUserProfile", routeValues: new { userId });

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("user/{userId}/reset-avatar")]
        [HttpPost]
        public async Task<IActionResult> SetDefaultUserAvatar(Guid userId, string returnUrl)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var response = await httpClient.GetAsync($"/api/User/reset-avatar/{userId}");
            response.EnsureSuccessStatusCode();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("EditUserProfile", new { userId });
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/update-username")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserName(Guid userId, UpdateUserNameDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new 
                    { UserId = userId, model.NewUserName}), Encoding.UTF8, "application/json");

                var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{userId}");
                userResponse.EnsureSuccessStatusCode();
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
                string refreshToken = user!.RefreshToken!;

                var updateUserNameResponse = await httpClient.PutAsync("/api/User/update-user-name", jsonContent);
                if (updateUserNameResponse.StatusCode == HttpStatusCode.Conflict)
                    return Conflict("Пользователь с таким именем уже существует");
                updateUserNameResponse.EnsureSuccessStatusCode();

                string protectedAccessToken = Request.Cookies[CookieNames.AccessToken]!;
                string accessToken = dataProtector.Unprotect(protectedAccessToken);
                using StringContent refreshTokenJsonContent = new(JsonSerializer.Serialize(new
                    { AccessToken = accessToken, RefreshToken = refreshToken }), Encoding.UTF8, "application/json");
                var refreshTokenResponse = await httpClient.PostAsync("/api/Token/refresh", refreshTokenJsonContent);
                refreshTokenResponse.EnsureSuccessStatusCode();
                string newAccessToken = await refreshTokenResponse.Content.ReadAsStringAsync();
                string protectedNewAccessToken = dataProtector.Protect(newAccessToken);
                Response.Cookies.Append(CookieNames.AccessToken, protectedNewAccessToken, new CookieOptions
                    { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });

                return Ok();
            }
            
            return BadRequest("Заполните все поля");
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/check-password")]
        [HttpPost]
        public async Task<IActionResult> CheckUserPassword(Guid userId, [FromForm] CheckUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new { model.Password }), Encoding.UTF8, "application/json");

                var checkPasswordResponse = await httpClient.PostAsync($"/api/User/check-password/{userId}", jsonContent);
                checkPasswordResponse.EnsureSuccessStatusCode();
                bool result = await checkPasswordResponse.Content.ReadFromJsonAsync<bool>();

                return Ok(result);
            }

            return BadRequest(new
                { errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }

        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("users/{userId}/update-password")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserPassword(Guid userId, [FromForm] UpdateUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new { model.NewPassword }),
                    Encoding.UTF8, "application/json");

                var updateUserPasswordResponse = await httpClient.PutAsync($"/api/User/update-user-password/{userId}", jsonContent);
                updateUserPasswordResponse.EnsureSuccessStatusCode();

                return Ok();
            }

            return BadRequest(new 
                { errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }
    }
}
