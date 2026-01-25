using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.User
{
    public class CheckUserPasswordDto
    {
        [Required]
        public string Password { get; set; }
    }
}
