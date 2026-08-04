using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.RejectReviewAndAddRestriction
{
    public class UserFailedToBeRestrictedSagaEvent : MessageBase
    {
        public Guid ReviewId { get; set; }
    }
}
