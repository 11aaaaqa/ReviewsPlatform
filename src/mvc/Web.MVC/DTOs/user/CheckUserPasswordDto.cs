using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.user
{
    public class CheckUserPasswordDto
    {
        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string Password { get; set; }
    }
}
