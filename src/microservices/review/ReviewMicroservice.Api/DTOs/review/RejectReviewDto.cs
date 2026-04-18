using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class RejectReviewDto
    {
        [Required]
        public Guid ReviewId { get; set; }

        [Required]
        [StringLength(500)]
        public string RejectionReason { get; set; }
    }
}
