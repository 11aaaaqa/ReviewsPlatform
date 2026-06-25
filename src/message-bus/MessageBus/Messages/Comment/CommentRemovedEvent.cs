using MessageBus.Abstractions;

namespace MessageBus.Messages.Comment
{
    public class CommentRemovedEvent : MessageBase
    {
        public List<Guid> CommentIds { get; set; } = new();
    }
}
