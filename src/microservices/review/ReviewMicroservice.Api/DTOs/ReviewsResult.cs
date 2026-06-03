using ReviewMicroservice.Api.Models;

namespace ReviewMicroservice.Api.DTOs
{
    public class ReviewsResult
    {
        public List<ReviewNoPictures> Reviews { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
