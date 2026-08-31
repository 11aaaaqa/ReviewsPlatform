using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.admin
{
    public class RejectAllReviewsByUserIdDto
    {
        [Required(ErrorMessage = "Заполните все поля")]
        [StringLength(StringLengthDtoConstants.ReviewRejectReasonMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string RejectionReason { get; set; }
    }
}
