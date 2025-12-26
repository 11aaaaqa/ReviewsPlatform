namespace AccountMicroservice.Api.Models.Business
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
