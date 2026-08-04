using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.category
{
    public class AddSubcategoryDto
    {
        [Required(ErrorMessage = "Поле \"Название подкатегории\" обязательно")]
        [StringLength(StringLengthDtoConstants.SubcategoryNameMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string Name { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
