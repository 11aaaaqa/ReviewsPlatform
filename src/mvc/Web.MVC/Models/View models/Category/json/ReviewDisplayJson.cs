using Web.MVC.Models.View_models.Review;

namespace Web.MVC.Models.View_models.Category.json
{
    public class ReviewDisplayJson
    {
        public List<ReviewDisplay> Reviews { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
