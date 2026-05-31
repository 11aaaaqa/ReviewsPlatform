using Web.MVC.Models.Api_responses.review.enums;
using Web.MVC.Models.View_models.Category;

namespace Web.MVC.Models.View_models.Review
{
    public class GetReviewsByItemIdViewModel
    {
        public ItemDisplay Item { get; set; }
        public List<ReviewDisplay> Reviews { get; set; }
        public bool IsNextPageExisted { get; set; }
        public int PageSize { get; set; }

        public OrderByDate? Date { get; set; }
        public OrderByEstimation? Estimation { get; set; }
        public int? Stars { get; set; }
    }
}
