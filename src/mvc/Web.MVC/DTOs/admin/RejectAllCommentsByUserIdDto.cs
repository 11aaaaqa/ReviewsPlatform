using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.admin
{
    public class RejectAllCommentsByUserIdDto
    {
        [Required(ErrorMessage = "Заполните все поля")]
        [StringLength(StringLengthDtoConstants.CommentRejectReasonMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string RejectionReason { get; set; }
    }
}
