using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.MVC.Constants;
using Web.MVC.DTOs.admin;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.View_models.Admin;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Controllers
{
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public AdminController(ILogger<AdminController> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
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
    }
}
