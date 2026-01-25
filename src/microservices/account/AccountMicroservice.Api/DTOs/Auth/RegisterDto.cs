using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AccountMicroservice.Api.DTOs.Auth
{
    public class RegisterDto
    {
        private string userName;
        [Required]
        [StringLength(30)]
        public string UserName
        {
            get => userName;
            set => userName = value != null ? Regex.Replace(value.Trim(), @"\s+", " ") : null;
        }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
