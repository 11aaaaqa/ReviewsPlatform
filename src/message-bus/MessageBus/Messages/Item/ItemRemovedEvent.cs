using MessageBus.Abstractions;

namespace MessageBus.Messages.Item
{
    public class ItemRemovedEvent : MessageBase
    {
        public Guid ItemId { get; set; }
    }
}
