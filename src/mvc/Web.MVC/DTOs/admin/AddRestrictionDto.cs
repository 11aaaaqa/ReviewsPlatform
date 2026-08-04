using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;
using Web.MVC.Models.Api_responses.restriction.enums;

namespace Web.MVC.DTOs.admin
{
    public class AddRestrictionDto
    {
        [Required(ErrorMessage = "Поле \"Тип блокировки\" обязательно")]
        public RestrictionType RestrictionType { get; set; }

        public int DurationInDays { get; set; } = 0;

        [Required]
        public bool IsPermanent { get; set; }

        [Required(ErrorMessage = "Поле \"Причина блокировки\" обязательно")]
        [StringLength(StringLengthDtoConstants.RestrictionReasonMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string Reason { get; set; }
    }
}
