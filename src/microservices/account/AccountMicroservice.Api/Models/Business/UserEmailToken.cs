using AccountMicroservice.Api.Enums;

namespace AccountMicroservice.Api.Models.Business
{
    public class UserEmailToken
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public EmailTokenPurpose TokenPurpose { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
