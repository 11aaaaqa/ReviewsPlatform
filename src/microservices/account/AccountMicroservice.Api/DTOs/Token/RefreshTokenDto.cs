using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.Token
{
    public class RefreshTokenDto
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
