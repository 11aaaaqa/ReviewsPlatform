using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.CreateItemWIthReview
{
    public class ReviewFailedToCreateSagaEvent : MessageBase
    {
        public Guid ItemId { get; set; }
    }
}
