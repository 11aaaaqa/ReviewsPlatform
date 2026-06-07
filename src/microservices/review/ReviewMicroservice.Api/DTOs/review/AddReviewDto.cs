using System.ComponentModel.DataAnnotations;
using Ganss.Xss;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class AddReviewDto
    {
        private static readonly HtmlSanitizer htmlSanitizer = new();

        [Required]
        [StringLength(500)]
        public string ShortReview
        {
            get => shortReview;
            set => shortReview = htmlSanitizer.Sanitize(value);
        }
        private string shortReview;

        [Required]
        [StringLength(2000)]
        public string Text
        {
            get => text;
            set => text = htmlSanitizer.Sanitize(value);
        }

        private string text;

        [Required]
        public int ItemEstimation { get; set; }

        [Required]
        public Guid ItemId { get; set; }

        public List<byte[]> Pictures { get; set; } = new();
    }
}
