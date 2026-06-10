using Web.MVC.Models.Api_responses.review.enums;

namespace Web.MVC.Models.View_models.Review
{
    public class GetReviewByIdViewModel
    {
        public ReviewDisplay Review { get; set; }
        public ReactionType? ReactionType { get; set; }
        public bool ShowEmailConfirmationModalOnReaction { get; set; }
        public bool IsReviewCreatedByCurrentUser { get; set; }
        public string EncodedCurrentUrl { get; set; }
    }
}
