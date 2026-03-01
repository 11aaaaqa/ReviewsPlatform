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
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
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
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var allRolesResponse = await httpClient.GetAsync("/api/Role/all");
            allRolesResponse.EnsureSuccessStatusCode();
            var allRoles = await allRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();

            using StringContent jsonContent = new(JsonSerializer.Serialize(new { UserId = userId, RoleIds = roleIds }),
                Encoding.UTF8, "application/json");

            var setUserRolesResponse = await httpClient.PostAsync("/api/User/set-user-roles", jsonContent);
            setUserRolesResponse.EnsureSuccessStatusCode();

            List<string> assignedRoleNames = allRoles!.Where(x => roleIds.Contains(x.Id)).Select(x => x.Name).ToList();
            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {AdminId} set roles to user {UserId}: {Roles}", currentUserIdStr, userId, assignedRoleNames);

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
                    logger.LogCritical("User, with Id {UserId} that has been extracted from user's claim, is not in Users database", userId);
                    return StatusCode((int)HttpStatusCode.NotFound);
                }
                logger.LogCritical("Something went wrong while trying to get user with Id {UserId}", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

            string avatarSrc = avatarConverter.GetAvatarSrc(user!.AvatarSource);

            return View(new EditUserProfileViewModel
            {
                AvatarSrc = avatarSrc, UserEmail = user.Email, UserId = userId, IsAvatarDefault = user.IsAvatarDefault,
                UserName = user.UserName, IsEmailVerified = user.IsEmailVerified
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
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var response = await httpClient.PutAsync($"/api/User/set-avatar/{userId}", jsonContent);
            response.EnsureSuccessStatusCode();

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction("EditUserProfile");

            return LocalRedirect(returnUrl);
        }

        [Authorize]
        [Route("settings/reset-avatar")]
        [HttpPost]
        public async Task<IActionResult> SetDefaultUserAvatar(string returnUrl)
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var response = await httpClient.GetAsync($"/api/User/reset-avatar/{userId}");
            response.EnsureSuccessStatusCode();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("EditUserProfile");
        }

        [Authorize]
        [Route("settings/update-username")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserName(UpdateUserNameDto model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new 
                    { model.NewUserName}), Encoding.UTF8, "application/json");

                var updateUserNameResponse = await httpClient.PutAsync($"/api/User/update-user-name/{userId}", jsonContent);

                if (updateUserNameResponse.StatusCode == HttpStatusCode.Conflict)
                    return Conflict("Пользователь с таким именем уже существует");
                updateUserNameResponse.EnsureSuccessStatusCode();

                string accessToken = await updateUserNameResponse.Content.ReadAsStringAsync();
                
                SaveAccessToken(accessToken);

                return RedirectToAction("EditUserProfile");
            }
            
            return BadRequest("Заполните все поля");
        }

        [Authorize]
        [Route("settings/password/request-update")]
        [HttpPost]
        public async Task<IActionResult> RequestPasswordUpdate([FromForm] RequestPasswordUpdateDto model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

                using StringContent checkPasswordJsonContent = new(JsonSerializer.Serialize(new
                {
                    Password = model.UserPassword
                }), Encoding.UTF8, "application/json");
                var checkPasswordResponse = await httpClient.PostAsync($"/api/User/check-password/{userId}", checkPasswordJsonContent);
                checkPasswordResponse.EnsureSuccessStatusCode();
                bool checkPasswordResult = await checkPasswordResponse.Content.ReadFromJsonAsync<bool>();
                if (!checkPasswordResult)
                    return BadRequest(new { errorMessages = new List<string>{ "Неверный пароль" } });

                HttpClient httpClientNoToken = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent sendResetTokenJsonContent = new(JsonSerializer.Serialize(new
                {
                    Url = $"{configuration["CurrentUrl:Scheme"]}://{configuration["CurrentUrl:Domain"]}/users/{userId}/password/update?token="
                }), Encoding.UTF8, "application/json");
                var sendResetTokenResponse = await httpClientNoToken.PostAsync(
                    $"/api/User/send-password-reset-token/{userId}", sendResetTokenJsonContent);
                if (!sendResetTokenResponse.IsSuccessStatusCode)
                {
                    if (sendResetTokenResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, "Смена пароля уже запрошена");
                    }
                    if (sendResetTokenResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return BadRequest(new { errorMessages = new List<string>{ "Перед сменой пароля необходимо подтвердить почту" } });
                    }
                    sendResetTokenResponse.EnsureSuccessStatusCode();
                }

                return Ok();
            }

            return BadRequest(new
                { errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }

        [AllowAnonymous]
        [Route("users/{userId}/password/update")] //изменение маршрута влияет на методы RequestPasswordUpdate и ForgetPassword
        [HttpGet]
        public IActionResult UpdateUserPassword(Guid userId, string token)
        {
            return View(new UpdateUserPasswordDto{ UserId = userId, Token = token });
        }

        [AllowAnonymous]
        [Route("users/{userId}/password/update")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserPassword([FromForm] UpdateUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
                using StringContent jsonContent = new(JsonSerializer.Serialize(new { model.NewPassword, model.Token }),
                    Encoding.UTF8, "application/json");
                var updateUserPasswordResponse = await httpClient.PostAsync($"/api/User/update-user-password/{model.UserId}", jsonContent);
                if (!updateUserPasswordResponse.IsSuccessStatusCode)
                {
                    if (updateUserPasswordResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError(string.Empty, "Время действия токена истекло");
                        return View(model);
                    }
                    updateUserPasswordResponse.EnsureSuccessStatusCode();
                }

                if (User.Identity.IsAuthenticated)
                {
                    var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-id/{model.UserId}");
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        Response.Cookies.Delete(CookieNames.AccessToken);
                        ModelState.AddModelError(string.Empty, "Пароль был успешно изменен, требуется перезайти в аккаунт");
                        logger.LogCritical("User {UserId} updated his password but something went wrong during recreating access token for him", model.UserId);
                        return View();
                    }
                    var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                    using StringContent loginJsonContent = new(JsonSerializer.Serialize(new 
                        { UserNameOrEmail = user!.Email, Password = model.NewPassword }), Encoding.UTF8, "application/json");
                    var loginResponse = await httpClient.PostAsync("/api/Auth/login", loginJsonContent);
                    if (!loginResponse.IsSuccessStatusCode)
                    {
                        Response.Cookies.Delete(CookieNames.AccessToken);
                        ModelState.AddModelError(string.Empty, "Пароль был успешно изменен, требуется перезайти в аккаунт");
                        logger.LogCritical("User {UserId} updated his password but something went wrong during recreating access token for him", model.UserId);
                        return View();
                    }
                    string accessToken = await loginResponse.Content.ReadAsStringAsync();
                    SaveAccessToken(accessToken);
                    return RedirectToAction("EditUserProfile");
                }

                return RedirectToAction("Index","Home");
            }

            return View(model);
        }

        [Authorize]
        [Route("settings/complete-sign-out")]
        [HttpPost]
        public async Task<IActionResult> SignOutFromAllDevices()
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var revokeResponse = await httpClient.GetAsync($"/api/Token/revoke/{userId}");
            revokeResponse.EnsureSuccessStatusCode();

            Response.Cookies.Delete(CookieNames.AccessToken);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [Route("settings/email/request-confirmation")]
        [HttpPost]
        public async Task<IActionResult> RequestEmailConfirmation(string returnUrl)
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            { 
                Url = $"{configuration["CurrentUrl:Scheme"]}://{configuration["CurrentUrl:Domain"]}/settings/email/confirm?token="
            }), Encoding.UTF8, "application/json");

            var sendTokenResponse = await httpClient.PostAsync($"/api/User/send-email-confirmation-token/{userId}",
                jsonContent);
            if (!sendTokenResponse.IsSuccessStatusCode)
            {
                if (sendTokenResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, "Вы запросили максимальное количество кодов, попробуйте позже");
                }
                sendTokenResponse.EnsureSuccessStatusCode();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("EditUserProfile");
        }

        [Authorize]
        [Route("settings/email/confirm")] //при изменении влияет на работу метода RequestEmailConfirmation
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            Guid userId = new Guid(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);

            var confirmEmailResponse = await httpClient.GetAsync($"/api/User/confirm-user-email/{userId}?token={token}");
            confirmEmailResponse.EnsureSuccessStatusCode();

            return RedirectToAction("EditUserProfile");
        }

        [AllowAnonymous]
        [Route("password/reset")]
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            if (User.Identity.IsAuthenticated) return Forbid();

            return View(new ForgetPasswordDto{ IsRequested = false });
        }

        [AllowAnonymous]
        [Route("password/reset")]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto model)
        {
            if (User.Identity.IsAuthenticated) return Forbid();

            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);

                var userResponse = await httpClient.GetAsync($"/api/User/get-user-by-email?email={model.Email}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    if (userResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        ModelState.AddModelError(string.Empty, "Пользователь не найден");
                        return View(model);
                    }
                    userResponse.EnsureSuccessStatusCode();
                }
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                using StringContent sendResetTokenJsonContent = new(JsonSerializer.Serialize(new
                {
                    Url = $"{configuration["CurrentUrl:Scheme"]}://{configuration["CurrentUrl:Domain"]}/users/{user!.Id}/password/update?token="
                }), Encoding.UTF8, "application/json");
                var sendResetTokenResponse = await httpClient.PostAsync(
                    $"/api/User/send-password-reset-token/{user.Id}", sendResetTokenJsonContent);
                if (!sendResetTokenResponse.IsSuccessStatusCode)
                {
                    if (sendResetTokenResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        ModelState.AddModelError(string.Empty, "Смена пароля уже запрошена");
                        return View(model);
                    }
                    if (sendResetTokenResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError(string.Empty, "Перед сменой пароля необходимо подтвердить почту");
                        return View(model);
                    }
                    sendResetTokenResponse.EnsureSuccessStatusCode();
                }

                return View(new ForgetPasswordDto{ IsRequested = true });
            }
            return View(model);
        }


        private void SaveAccessToken(string unprotectedAccessToken)
        {
            HttpContext.Items[CookieNames.AccessToken] = unprotectedAccessToken;
            string protectedAccessToken = dataProtector.Protect(unprotectedAccessToken);
            Response.Cookies.Append(CookieNames.AccessToken, protectedAccessToken, new CookieOptions
                { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax });
        }
    }
}
