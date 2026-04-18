using MessageBus.Abstractions;

namespace MessageBus.Messages.Review
{
    public class ReviewAcceptedEvent : MessageBase
    {
        public Guid ItemId { get; set; }
        public bool IsReviewCreatedWithItem { get; set; }
        public int ItemEstimation { get; set; }
    }
}
