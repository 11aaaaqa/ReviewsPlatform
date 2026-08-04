using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.RejectReviewAndAddRestriction
{
    public class UserRestrictedSagaEvent : MessageBase
    {
        public Guid ReviewId { get; set; }
    }
}
