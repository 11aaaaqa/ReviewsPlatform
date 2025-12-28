using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.account
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Поле \"Имя пользователя или адрес эл. почты\" обязательно")]
        [Display(Name = "Имя пользователя или адрес эл. почты")]
        [StringLength(200)]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
