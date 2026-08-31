using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.admin
{
    public class DisableUserRestrictionDto
    {
        [Required(ErrorMessage = "Заполните все поля")]
        [StringLength(StringLengthDtoConstants.DisableRestrictionReasonMax)]
        public string DisablingReason { get; set; }
    }
}
