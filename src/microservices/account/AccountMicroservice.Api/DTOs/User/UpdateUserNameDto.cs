namespace AccountMicroservice.Api.DTOs.User
{
    public class UpdateUserNameDto
    {
        public Guid UserId { get; set; }
        public string NewUserName { get; set; }
    }
}
