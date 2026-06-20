using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.comment
{
    public class RejectCommentDto
    {
        [Required]
        public Guid CommentId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
