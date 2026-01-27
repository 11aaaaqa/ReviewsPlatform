using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Web.MVC.Constants;
using Web.MVC.DTOs.user;
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
        private readonly IConfiguration configuration;
        private readonly List<string> availableFileExtensions = new() { ".jpg", ".png", ".jpeg"};
        public UserController(IHttpClientFactory httpClientFactory, ILogger<UserController> logger,
            AvatarConverter avatarConverter, IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposeConstants.Jwt);
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.avatarConverter = avatarConverter;
            this.configuration = configuration;
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
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var allRolesResponse = await httpClient.GetAsync("/api/Role/all");
            allRolesResponse.EnsureSuccessStatusCode();
            var allRoles = await allRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();

            using StringContent jsonContent = new(JsonSerializer.Serialize(new { UserId = userId, RoleIds = roleIds }),
                Encoding.UTF8, "application/json");

            var setUserRolesResponse = await httpClient.PostAsync("/api/User/set-user-roles", jsonContent);
            setUserRolesResponse.EnsureSuccessStatusCode();

            List<string> assignedRoleNames = allRoles!.Where(x => roleIds.Contains(x.Id)).Select(x => x.Name).ToList();
            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("{Timestamp}: User {AdminId} set roles to user {UserId}: {Roles}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), currentUserIdStr, userId, assignedRoleNames);

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [Route("settings")]
        [HttpGet]
        public async Task<IActionResult> EditUserProfile()
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
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
        [Route("settings/update-avatar")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserAvatar(UpdateAvatarDto model, string returnUrl)
        {
            using MemoryStream memoryStream = new MemoryStream();
            await model.Image.CopyToAsync(memoryStream);
            if (memoryStream.Length > 2 * 1024 * 1024)
                return StatusCode((int)HttpStatusCode.BadRequest, "Размер файла превышает 2 мб");
            
            if (!availableFileExtensions.Any(x => x == Path.GetExtension(model.Image.FileName)))
                return StatusCode((int)HttpStatusCode.BadRequest, "Неверный формат");
            
            byte[] avatarSource = memoryStream.ToArray();
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            { AvatarSource = avatarSource }), Encoding.UTF8, "application/json");
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var response = await httpClient.PutAsync($"/api/User/set-avatar/{userId}", jsonContent);
            response.EnsureSuccessStatusCode();

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction("EditUserProfile", routeValues: new { userId });

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [Route("settings/reset-avatar")]
        [HttpPost]
        public async Task<IActionResult> SetDefaultUserAvatar(string returnUrl)
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var response = await httpClient.GetAsync($"/api/User/reset-avatar/{userId}");
            response.EnsureSuccessStatusCode();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("EditUserProfile", new { userId });
        }

        [Authorize]
        [Route("settings/update-username")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserName(UpdateUserNameDto model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new 
                    { model.NewUserName}), Encoding.UTF8, "application/json");

                string secret = configuration["InternalEndpoint:Secret"]!;
                var getRefreshTokenResponse = await httpClient.GetAsync($"/api/User/get-refresh-token/{userId}?secret={secret}");
                getRefreshTokenResponse.EnsureSuccessStatusCode();
                var refreshTokenFormat = await getRefreshTokenResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();

                var updateUserNameResponse = await httpClient.PutAsync($"/api/User/update-user-name/{userId}", jsonContent);
                if (updateUserNameResponse.StatusCode == HttpStatusCode.Conflict)
                    return Conflict("Пользователь с таким именем уже существует");
                updateUserNameResponse.EnsureSuccessStatusCode();

                string protectedAccessToken = Request.Cookies[CookieNames.AccessToken]!;
                string accessToken = dataProtector.Unprotect(protectedAccessToken);
                using StringContent refreshTokenJsonContent = new(JsonSerializer.Serialize(new
                    { AccessToken = accessToken, refreshTokenFormat!.RefreshToken }), Encoding.UTF8, "application/json");
                var refreshTokenResponse = await httpClient.PostAsync("/api/Token/refresh", refreshTokenJsonContent);
                refreshTokenResponse.EnsureSuccessStatusCode();
                string newAccessToken = await refreshTokenResponse.Content.ReadAsStringAsync();
                SaveAccessToken(newAccessToken);

                return Ok();
            }
            
            return BadRequest("Заполните все поля");
        }

        [Authorize]
        [Route("settings/check-password")]
        [HttpPost]
        public async Task<IActionResult> CheckUserPassword([FromForm] CheckUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
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
        [Route("settings/update-password")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserPassword([FromForm] UpdateUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

                var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{userId}");
                userResponse.EnsureSuccessStatusCode();
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                using StringContent jsonContent = new(JsonSerializer.Serialize(new { model.NewPassword, model.OldPassword }),
                    Encoding.UTF8, "application/json");

                var updateUserPasswordResponse = await httpClient.PutAsync($"/api/User/update-user-password/{userId}", jsonContent);
                updateUserPasswordResponse.EnsureSuccessStatusCode();

                using StringContent loginJsonContent = new(JsonSerializer.Serialize(new
                {
                    UserNameOrEmail = user!.Email, Password = model.NewPassword
                }), Encoding.UTF8, "application/json");
                var loginResponse = await httpClient.PostAsync("/api/Auth/login", loginJsonContent);
                loginResponse.EnsureSuccessStatusCode();
                string accessToken = await loginResponse.Content.ReadAsStringAsync();

                SaveAccessToken(accessToken);

                return Ok();
            }

            return BadRequest(new 
                { errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }

        [Authorize]
        [Route("settings/complete-sign-out")]
        [HttpPost]
        public async Task<IActionResult> SignOutFromAllDevices()
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

            var revokeResponse = await httpClient.GetAsync($"/api/Token/revoke/{userId}");
            revokeResponse.EnsureSuccessStatusCode();

            Response.Cookies.Delete(CookieNames.AccessToken);

            return RedirectToAction("Index", "Home");
        }

        private void SaveAccessToken(string unprotectedAccessToken)
        {
            string protectedAccessToken = dataProtector.Protect(unprotectedAccessToken);
            Response.Cookies.Append(CookieNames.AccessToken, protectedAccessToken, new CookieOptions
                { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
        }
    }
}
