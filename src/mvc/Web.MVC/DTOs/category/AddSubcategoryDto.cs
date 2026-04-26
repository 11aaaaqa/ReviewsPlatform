using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.category
{
    public class AddSubcategoryDto
    {
        [Required]
        [StringLength(StringLengthDtoConstants.SubcategoryName)]
        public string Name { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
