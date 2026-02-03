using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.user
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Поле \"Адрес эл. почты\" обязательно")]
        [Display(Name = "Адрес эл. почты")]
        [DataType(DataType.EmailAddress)]
        [StringLength(200)]
        public string Email { get; set; }

        public bool IsRequested { get; set; } = false;
    }
}
