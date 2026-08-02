using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.category
{
    public class AddCategoryDto
    {
        [Required(ErrorMessage = "Поле \"Название категории\" обязательно")]
        [RegularExpression("^[A-ZА-ЯЁ][a-zа-яё]*(?: [a-zа-яё]+){0,2}$", ErrorMessage = "Название не подходит по шаблону")]
        [StringLength(StringLengthDtoConstants.CategoryNameMax, ErrorMessage = "Превышено максимальное количество символов")]
        public string Name { get; set; }
    }
}
