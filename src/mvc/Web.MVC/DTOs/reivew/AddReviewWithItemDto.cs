using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.reivew
{
    public class AddReviewWithItemDto
    {
        [StringLength(StringLengthDtoConstants.ItemNameMax)]
        [Required(ErrorMessage = "Поле \"Название товара\" обязательно")]
        [Display(Name = "Название товара")]
        public string ItemName { get; set; }

        [StringLength(StringLengthDtoConstants.ItemBrandMax)]
        [Display(Name = "Бренд")]
        public string? ItemBrand { get; set; }

        [Required(ErrorMessage = "Поле \"Фото товара\" обязательно")]
        [Display(Name = "Фото товара")]
        public IFormFile ItemPicture { get; set; }

        [Required]
        public Guid SubcategoryId { get; set; }



        [Required(ErrorMessage = "Поле \"Краткое резюме\" обязательно")]
        [StringLength(StringLengthDtoConstants.ShortReviewMax)]
        [Display(Name = "Краткое резюме")]
        public string ShortReview { get; set; }

        [Required(ErrorMessage = "Поле \"Отзыв\" обязательно")]
        [StringLength(StringLengthDtoConstants.ReviewTextMax)]
        [Display(Name = "Отзыв")]
        public string ReviewText { get; set; }

        [Required(ErrorMessage = "Поле \"Оценка\" обязательно")]
        [Display(Name = "Оценка")]
        public int ReviewItemEstimation { get; set; }

        public List<IFormFile> ReviewPictures { get; set; } = new();
    }
}
