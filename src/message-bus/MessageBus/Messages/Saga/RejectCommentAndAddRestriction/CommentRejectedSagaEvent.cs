using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.RejectCommentAndAddRestriction
{
    public class CommentRejectedSagaEvent : MessageBase
    {
        public Guid RestrictedUserId { get; set; }
        public Guid RestrictingUserId { get; set; }
        public int RestrictionType { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsPermanent { get; set; }
        public string Reason { get; set; }

        public Guid CommentId { get; set; }
    }
}
