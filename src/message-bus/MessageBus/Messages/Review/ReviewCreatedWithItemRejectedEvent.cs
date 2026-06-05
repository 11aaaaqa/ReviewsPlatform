using MessageBus.Abstractions;

namespace MessageBus.Messages.Review
{
    public class ReviewCreatedWithItemRejectedEvent : MessageBase
    {
        public Guid ItemId { get; set; }
    }
}
