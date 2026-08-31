using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;
using Web.MVC.Models.Api_responses.restriction.enums;

namespace Web.MVC.DTOs.admin
{
    public class AddUserRestrictionDto
    {
        [Required(ErrorMessage = "Поле \"Тип блокировки\" обязательно")]
        public RestrictionType RestrictionType { get; set; }

        [Required(ErrorMessage = "Поле \"Длительность\" обязательно")]
        public int DurationDays { get; set; } = 0;

        [Required(ErrorMessage = "Поле \"Перманентно\" обязательно")]
        public bool IsPermanent { get; set; }

        [Required(ErrorMessage = "Поле \"Причина\" обязательно")]
        [StringLength(StringLengthDtoConstants.RestrictionReasonMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string Reason { get; set; }
    }
}
