using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string url;
        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
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
            var model = new GetUserByIdViewModel { User = user, CanUserSetTheRoles = canUserSetTheRoles, CanUserViewTheRoles = canUserViewTheRoles};
            if (canUserSetTheRoles)
            {
                var allRolesResponse = await httpClient.GetAsync($"{url}/api/Role/all");
                allRolesResponse.EnsureSuccessStatusCode();
                var allRoles = await allRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
                model.AllRoles = allRoles;
            }

            return View(model);
        }

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
    }
}
