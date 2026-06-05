using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.account
{
    public class RegisterDto
    {
        private string userName;

        [Required(ErrorMessage = "Поле \"Имя пользователя\" обязательно")]
        [Display(Name = "Имя пользователя")]
        [StringLength(StringLengthDtoConstants.UserNameMax)]
        public string UserName
        {
            get => userName;
            set => userName = value != null ? Regex.Replace(value.Trim(), @"\s+", " ") : null;
        }

        [Required(ErrorMessage = "Поле \"Адрес эл. почты\" обязательно")]
        [Display(Name = "Адрес эл. почты")]
        [DataType(DataType.EmailAddress)]
        [StringLength(StringLengthDtoConstants.EmailAddressMax)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(StringLengthDtoConstants.PasswordMax, MinimumLength = StringLengthDtoConstants.PasswordMin,
            ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле \"Подтвердите пароль\" обязательно")]
        [Display(Name = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [StringLength(StringLengthDtoConstants.PasswordMax)]
        public string ConfirmPassword { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
