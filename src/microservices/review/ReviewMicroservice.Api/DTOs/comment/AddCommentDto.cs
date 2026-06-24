using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.comment
{
    public class AddCommentDto
    {
        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        public Guid ReviewId { get; set; }
    }
}
