using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Category
{
    public class AddCategoryDto
    {
        [Required]
        [RegularExpression("^[A-ZА-ЯЁ][a-zа-яё]*(?: [a-zа-яё]+){0,2}$")]
        [StringLength(25)]
        public string Name { get; set; }
    }
}
