using Ganss.Xss;
using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class RejectReviewDto
    {
        private static readonly HtmlSanitizer htmlSanitizer = new();

        [Required]
        public Guid ReviewId { get; set; }

        [Required]
        [StringLength(500)]
        public string RejectionReason
        {
            get => rejectionReason;
            set => rejectionReason = htmlSanitizer.Sanitize(value);
        }
        private string rejectionReason;

        public AddRestrictionDto? AddRestriction { get; set; }
    }
}
