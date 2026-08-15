using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.admin
{
    public class RejectCommentDto
    {
        [Required(ErrorMessage = "Поле \"Причина отклонения\" обязательно")]
        [StringLength(StringLengthDtoConstants.CommentRejectReasonMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string RejectionReason { get; set; }

        public AddRestrictionDto? AddRestriction { get; set; }
    }
}
