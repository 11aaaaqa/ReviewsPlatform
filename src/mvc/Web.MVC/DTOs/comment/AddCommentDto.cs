using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.comment
{
    public class AddCommentDto
    {
        [Required(ErrorMessage = "Поле \"Комментарий\" обязательно")]
        [StringLength(StringLengthDtoConstants.CommentTextMax, ErrorMessage = "Превышено максимально количество символов")]
        public string Text { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        public Guid ReviewId { get; set; }
    }
}
