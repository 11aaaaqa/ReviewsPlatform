using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.user
{
    public class UpdateUserNameDto
    {
        private string newUserName;

        [Required(ErrorMessage = "Поле \"Имя пользователя\" обязательно")]
        [Display(Name = "Имя пользователя")]
        [StringLength(StringLengthDtoConstants.UserNameMax)]
        public string NewUserName
        {
            get => newUserName;
            set => newUserName = value != null ? Regex.Replace(value.Trim(), @"\s+", " ") : null;
        }
    }
}
