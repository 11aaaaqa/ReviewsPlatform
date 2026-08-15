using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.RejectCommentAndAddRestriction
{
    public class CommentUserFailedToBeRestrictedSagaEvent : MessageBase
    {
        public Guid CommentId { get; set; }
    }
}
