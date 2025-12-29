using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.account;

namespace Web.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly string url;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDataProtector dataProtector;
        public AccountController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IDataProtectionProvider dataProtectionFactory)
        {
            url = $"{configuration["ApiGateway:Protocol"]}://{configuration["ApiGateway:Domain"]}";
            this.httpClientFactory = httpClientFactory;
            dataProtector = dataProtectionFactory.CreateProtector("JWT");
        }

        [Route("account/signup")]
        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            return View(new RegisterDto{ReturnUrl = returnUrl});
        }

        [Route("account/signup")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient();
                using StringContent jsonContent = new(JsonSerializer.Serialize(new
                {
                    model.Id, model.Email, model.Password, model.UserName
                }), Encoding.UTF8, "application/json");

                var registerResponse = await httpClient.PostAsync($"{url}/api/Auth/register", jsonContent);
                if (registerResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким именем и (или) адресом эл. почты уже существует");
                    return View(model);
                }
                registerResponse.EnsureSuccessStatusCode();

                using StringContent authJsonContent = new(JsonSerializer.Serialize
                    (new { UserNameOrEmail = model.Email, model.Password}), Encoding.UTF8, "application/json");
                var authResponse = await httpClient.PostAsync($"{url}/api/Auth/login", authJsonContent);
                authResponse.EnsureSuccessStatusCode();
                string accessToken = await authResponse.Content.ReadAsStringAsync();

                SaveAccessToken(accessToken);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl);

                return RedirectToAction("Index","Home");
            }

            return View(model);
        }

        [HttpGet]
        [Route("account/signin")]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginDto{ReturnUrl = returnUrl});
        }

        [HttpPost]
        [Route("account/signin")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient();
                using StringContent jsonContent = new(JsonSerializer.Serialize(new
                {
                    model.UserNameOrEmail, model.Password
                }), Encoding.UTF8, "application/json");

                var authenticatedResponse = await httpClient.PostAsync($"{url}/api/Auth/login", jsonContent);
                if (authenticatedResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Данные неверны");
                    return View(model);
                }
                authenticatedResponse.EnsureSuccessStatusCode();

                string accessToken = await authenticatedResponse.Content.ReadAsStringAsync();

                SaveAccessToken(accessToken);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [Route("account/logout")]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            var revokeResponse = await httpClient.GetAsync($"{url}/api/Token/revoke/{userId}");
            revokeResponse.EnsureSuccessStatusCode();

            Response.Cookies.Delete(CookieNames.AccessToken);

            return LocalRedirect(returnUrl);
        }

        private void SaveAccessToken(string accessToken)
        {
            string protectedAccessToken = dataProtector.Protect(accessToken);

            Response.Cookies.Append(CookieNames.AccessToken, protectedAccessToken, new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
