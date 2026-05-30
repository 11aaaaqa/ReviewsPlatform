namespace Web.MVC.Models.Api_responses.review
{
    public class ReviewsResultResponse
    {
        public List<ReviewNoPicturesResponse> Reviews { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
