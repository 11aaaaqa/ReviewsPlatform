using MessageBus.Abstractions;

namespace MessageBus.Messages.Category
{
    public class CategoryRemovedEvent : MessageBase
    {
        public Guid CategoryId { get; set; }
        public List<Guid> ItemIdsOfRemovedCategory { get; set; } = new();
    }
}
