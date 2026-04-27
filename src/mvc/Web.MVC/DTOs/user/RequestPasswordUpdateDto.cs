using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.user
{
    public class RequestPasswordUpdateDto
    {
        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(StringLengthDtoConstants.PasswordMax, MinimumLength = StringLengthDtoConstants.PasswordMin,
            ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string UserPassword { get; set; }
    }
}
