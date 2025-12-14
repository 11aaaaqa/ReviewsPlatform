namespace AccountMicroservice.Api.DTOs.Auth
{
    public class LoginDto
    {
        public string UserNameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
