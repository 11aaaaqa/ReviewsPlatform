using Ganss.Xss;
using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class RejectAllUserReviewsDto
    {
        private static readonly HtmlSanitizer htmlSanitizer = new();

        [Required]
        [StringLength(500)]
        public string RejectionReason
        {
            get => rejectionReason;
            set => rejectionReason = htmlSanitizer.Sanitize(value);
        }
        private string rejectionReason;
    }
}
