using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.DTOs
{
    public class ReviewsResult
    {
        public List<Review> Reviews { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
