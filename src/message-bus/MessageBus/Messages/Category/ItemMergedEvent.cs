using MessageBus.Abstractions;

namespace MessageBus.Messages.Category
{
    public class ItemMergedEvent : MessageBase
    {
        public Guid ReviewId { get; set; }
        public Guid MergedItemId { get; set; }
    }
}
