using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.user
{
    public class UpdateUserPasswordDto
    {
        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Поле \"Старый пароль\" обязательно")]
        [Display(Name = "Старый пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Поле \"Подтвердите пароль\" обязательно")]
        [Display(Name = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
        [StringLength(100)]
        public string ConfirmNewPassword { get; set; }
    }
}
