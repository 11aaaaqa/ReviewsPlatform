namespace AccountMicroservice.Api.DTOs.User
{
    public class UpdateUserPasswordDto
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
