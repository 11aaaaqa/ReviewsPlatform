using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.User
{
    public class UpdateUserPasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}
