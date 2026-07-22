using Web.MVC.Models.Api_responses.account;

namespace Web.MVC.Models.View_models.Admin
{
    public class GetUsersListViewModel
    {
        public List<RoleResponse> Roles { get; set; }
        public int PageSize { get; set; }
    }
}
