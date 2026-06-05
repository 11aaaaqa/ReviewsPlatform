using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.account
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Поле \"Имя пользователя или адрес эл. почты\" обязательно")]
        [Display(Name = "Имя пользователя или адрес эл. почты")]
        [StringLength(StringLengthDtoConstants.UserNameOrEmailMax)]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обязательно")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(StringLengthDtoConstants.PasswordMax)]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
