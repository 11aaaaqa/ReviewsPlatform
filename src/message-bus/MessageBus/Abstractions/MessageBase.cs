namespace MessageBus.Abstractions
{
    public abstract class MessageBase
    {
        public Guid MessageId { get; init; } = Guid.NewGuid();
    }
}
