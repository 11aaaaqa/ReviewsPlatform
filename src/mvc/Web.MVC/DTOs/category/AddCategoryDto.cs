using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.category
{
    public class AddCategoryDto
    {
        [Required]
        [RegularExpression("^[A-ZА-ЯЁ][a-zа-яё]*(?: [a-zа-яё]+){0,2}$")]
        [StringLength(StringLengthDtoConstants.CategoryNameMax)]
        public string Name { get; set; }
    }
}
