using Ganss.Xss;
using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.comment
{
    public class RejectAllUserCommentsDto
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
