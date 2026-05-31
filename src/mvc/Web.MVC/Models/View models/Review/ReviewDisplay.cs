using Web.MVC.Models.Api_responses.review.enums;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Models.View_models.Review
{
    public class ReviewDisplay
    {
        public Guid Id { get; set; }
        public string ShortReview { get; set; }
        public string Text { get; set; }
        public int ItemEstimation { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }

        public Guid ItemId { get; set; }

        public UserDisplay User { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsCreatedWithItem { get; set; }
    }
}
