using Ganss.Xss;
using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.comment
{
    public class AddCommentDto
    {
        private static readonly HtmlSanitizer htmlSanitizer = new();

        [Required]
        [StringLength(500)]
        public string Text
        {
            get => text;
            set => text = htmlSanitizer.Sanitize(value);
        }
        private string text;

        public Guid? ParentCommentId { get; set; }

        [Required]
        public Guid ReviewId { get; set; }
    }
}
