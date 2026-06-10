using Web.MVC.Models.Api_responses.review.enums;

namespace Web.MVC.Models.Api_responses.review
{
    public class ReviewReactionResponse
    {
        public Guid UserId { get; set; }
        public Guid ReviewId { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}
