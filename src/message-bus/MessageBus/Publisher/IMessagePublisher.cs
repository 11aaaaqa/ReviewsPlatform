using MessageBus.Abstractions;

namespace MessageBus.Publisher
{
    public interface IMessagePublisher
    {
        Task PublishAsync(MessageBase messageBusEvent);
    }
}
