using Web.MVC.Models.Api_responses.account;

namespace Web.MVC.Models.View_models.User
{
    public class GetUserByIdViewModel
    {
        public UserResponse User { get; set; }
        public bool CanUserSetTheRoles { get; set; }
        public bool CanUserViewTheRoles { get; set; }
        public string AvatarSrc { get; set; }
        public List<RoleResponse> AllRoles { get; set; } = new();
    }
}
