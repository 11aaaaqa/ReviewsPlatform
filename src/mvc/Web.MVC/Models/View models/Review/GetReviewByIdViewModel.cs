using Web.MVC.Models.Api_responses.review.enums;

namespace Web.MVC.Models.View_models.Review
{
    public class GetReviewByIdViewModel
    {
        public ReviewDisplay Review { get; set; }
        public ReactionType? ReactionType { get; set; }
        public bool ShowEmailConfirmationModal { get; set; }
        public bool IsReviewCreatedByCurrentUser { get; set; }
        public string EncodedCurrentUrl { get; set; }
        public Guid? CurrentUserId { get; set; }
    }
}
