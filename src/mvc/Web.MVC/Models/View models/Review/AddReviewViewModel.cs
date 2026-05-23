using Web.MVC.DTOs.reivew;
using Web.MVC.Models.Api_responses.category;

namespace Web.MVC.Models.View_models.Review
{
    public class AddReviewViewModel
    {
        public ItemResponse Item { get; set; }
        public string ItemImageSrc { get; set; }
        public AddReviewDto AddReviewModel { get; set; }
    }
}
