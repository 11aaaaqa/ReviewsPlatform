using System.ComponentModel.DataAnnotations;
using Web.MVC.Constants;

namespace Web.MVC.DTOs.reivew
{
    public class AddReviewDto
    {
        [Required(ErrorMessage = "Поле \"Краткое резюме\" обязательно")]
        [StringLength(StringLengthDtoConstants.ShortReviewMax, ErrorMessage = "Превышено максимальное количество символов у поля \"Краткое резюме\"")]
        [Display(Name = "Краткое резюме")]
        public string ShortReview { get; set; }

        [Required(ErrorMessage = "Поле \"Отзыв\" обязательно")]
        [StringLength(StringLengthDtoConstants.ReviewTextMax, ErrorMessage = "Превышено максимальное количество символов у поля \"Отзыв\"")]
        [Display(Name = "Отзыв")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Поле \"Оценка\" обязательно")]
        [Display(Name = "Оценка")]
        public int ItemEstimation { get; set; }

        [Required]
        public Guid ItemId { get; set; }

        public List<IFormFile> Pictures { get; set; } = new();
    }
}
