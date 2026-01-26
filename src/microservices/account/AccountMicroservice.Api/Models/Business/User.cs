namespace AccountMicroservice.Api.Models.Business
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public byte[] AvatarSource { get; set; }
        public bool IsAvatarDefault { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public string? RefreshToken { get; set; }
        public int TokenVersion { get; set; } = 1;
        public DateTime RefreshTokenExpiryTime { get; set; }

        public List<Role> Roles { get; set; } = new();

        public override bool Equals(object? obj)
        {
            if (obj is User user)
                return Id == user.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
