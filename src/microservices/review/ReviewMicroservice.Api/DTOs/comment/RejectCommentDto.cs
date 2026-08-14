using Ganss.Xss;
using ReviewMicroservice.Api.DTOs.review;
using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.comment
{
    public class RejectCommentDto
    {
        private static readonly HtmlSanitizer htmlSanitizer = new();

        [Required]
        public Guid CommentId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason
        {
            get => reason;
            set => htmlSanitizer.Sanitize(value);
        }
        private string reason;

        public AddRestrictionDto? AddRestriction { get; set; }
    }
}
