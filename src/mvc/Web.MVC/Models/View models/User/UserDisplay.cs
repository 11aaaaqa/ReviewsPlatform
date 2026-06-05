using Web.MVC.Models.Api_responses.account;

namespace Web.MVC.Models.View_models.User
{
    public class UserDisplay
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string AvatarSrc { get; set; }
        public bool IsAvatarDefault { get; set; }
        public DateOnly RegistrationDate { get; set; }

        public List<RoleResponse> Roles { get; set; } = new();
    }
}
