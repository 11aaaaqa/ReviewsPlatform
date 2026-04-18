using System.ComponentModel.DataAnnotations;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class UpdateReviewDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        public string ShortReview { get; set; }
    }
}
