using Web.MVC.Models.Api_responses.review;

namespace Web.MVC.Models.View_models.Admin
{
    public class GetReviewsViewModel
    {
        public List<ReviewNoPicturesResponse> Reviews { get; set; }
        public bool IsNextPageExisted { get; set; }
        public int CurrentPageNumber { get; set; }
    }
}
