using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.CreateItemWIthReview
{
    public class ReviewCreatedSagaEvent : MessageBase
    {
        public Guid ItemId { get; set; }
    }
}
