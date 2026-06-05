using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class AddReviewDto
    {
        [Required]
        [StringLength(500)]
        public string ShortReview { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; }

        [Required]
        public int ItemEstimation { get; set; }

        [Required]
        public Guid ItemId { get; set; }

        public List<byte[]> Pictures { get; set; } = new();
    }
}
