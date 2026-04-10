using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Items
{
    public class AddItemAndReviewOnIt
    {
        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        [StringLength(100)]
        [Required]
        public string ItemName { get; set; }

        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        [StringLength(100)]
        public string? ItemBrand { get; set; }

        [Required]
        public IFormFile ItemPicture { get; set; }

        [Required]
        public Guid SubcategoryId { get; set; }



        [Required]
        [StringLength(200)]
        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        public string ShortReview { get; set; }

        [Required]
        [StringLength(2000)]
        public string ReviewText { get; set; }

        [Required]
        public int ReviewItemEstimation { get; set; }

        [Required]
        public List<IFormFile> ReviewPictures { get; set; } = new();
    }
}
