using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Subcategory
{
    public class AddSubcategoryDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
