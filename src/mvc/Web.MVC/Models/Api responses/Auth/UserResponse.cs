using StackExchange.Redis;

namespace Web.MVC.Models.Api_responses.Auth
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public List<Role> Roles { get; set; } = new();
    }
}
