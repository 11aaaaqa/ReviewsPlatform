using ReviewMicroservice.Api.Enums;

namespace ReviewMicroservice.Api.Models.Business
{
    public class ReviewReaction
    {
        public Guid UserId { get; set; }
        public Guid ReviewId { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}
