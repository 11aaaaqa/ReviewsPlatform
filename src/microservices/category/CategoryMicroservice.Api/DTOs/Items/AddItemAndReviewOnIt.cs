using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Items
{
    public class AddItemAndReviewOnIt
    {
        [StringLength(100)]
        [Required]
        public string ItemName { get; set; }

        [StringLength(100)]
        public string? ItemBrand { get; set; }

        [Required]
        public byte[] ItemPicture { get; set; }

        [Required]
        public Guid SubcategoryId { get; set; }



        [Required]
        [StringLength(200)]
        public string ShortReview { get; set; }

        [Required]
        [StringLength(2000)]
        public string ReviewText { get; set; }

        [Required]
        public int ReviewItemEstimation { get; set; }

        public List<byte[]> ReviewPictures { get; set; } = new();
    }
}
