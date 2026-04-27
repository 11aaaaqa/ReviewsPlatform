using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.user
{
    public class UpdateUserPasswordDto
    {
        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(StringLengthDtoConstants.PasswordMax, MinimumLength = StringLengthDtoConstants.PasswordMin,
            ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Поле \"Подтвердите пароль\" обязательно")]
        [Display(Name = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
        [StringLength(StringLengthDtoConstants.PasswordMax)]
        public string ConfirmNewPassword { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
