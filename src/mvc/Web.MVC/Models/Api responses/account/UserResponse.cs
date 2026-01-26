namespace Web.MVC.Models.Api_responses.account
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public byte[] AvatarSource { get; set; }
        public bool IsAvatarDefault { get; set; }
        public DateOnly RegistrationDate { get; set; }

        public List<RoleResponse> Roles { get; set; } = new();
    }
}
