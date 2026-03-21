using MessageBus.Abstractions;

namespace MessageBus.Messages.Category
{
    public class SubcategoryRemovedEvent : MessageBase
    {
        public Guid SubcategoryId { get; set; }
    }
}
