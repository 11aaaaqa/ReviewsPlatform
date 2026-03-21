using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Subcategory
{
    public class UpdateSubcategoryNameDto
    {
        [Required]
        [StringLength(50)]
        public string NewName { get; set; }

        public Guid SubcategoryId { get; set; }
    }
}
