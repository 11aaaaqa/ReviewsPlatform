using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.account
{
    public class RegisterDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Поле \"Имя пользователя\" обязательно")]
        [Display(Name = "Имя пользователя")]
        [StringLength(30)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле \"Адрес эл. почты\" обязательно")]
        [Display(Name = "Адрес эл. почты")]
        [DataType(DataType.EmailAddress)]
        [StringLength(200)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле \"Подтвердите пароль\" обязательно")]
        [Display(Name = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [StringLength(100)]
        public string ConfirmPassword { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
