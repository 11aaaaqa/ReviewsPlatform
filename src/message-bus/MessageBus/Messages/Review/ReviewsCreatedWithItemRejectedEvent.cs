using MessageBus.Abstractions;

namespace MessageBus.Messages.Review
{
    public class ReviewsCreatedWithItemRejectedEvent : MessageBase
    {
        public List<Guid> ItemIds { get; set; } = new();
    }
}
