using MessageBus.Abstractions;

namespace MessageBus.Messages.Comment
{
    public class CommentRemovedEvent : MessageBase
    {
        public Guid CommentId { get; set; }
    }
}
