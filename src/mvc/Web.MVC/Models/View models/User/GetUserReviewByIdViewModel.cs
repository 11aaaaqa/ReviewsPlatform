using Web.MVC.Models.Api_responses;
using Web.MVC.Models.View_models.Review;

namespace Web.MVC.Models.View_models.User
{
    public class GetUserReviewByIdViewModel
    {
        public ReviewDisplay Review { get; set; }
        public string? ReviewRejectionReason { get; set; }
        public EntityStatus ReviewStatus { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}
