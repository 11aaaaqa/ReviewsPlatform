using MessageBus.Abstractions;

namespace MessageBus.Messages.Review
{
    public class ReviewRemovedEvent : MessageBase
    {
        public Guid ItemId { get; set; }
        public bool IsReviewVerified { get; set; }
        public int ItemEstimation { get; set; }
    }
}
