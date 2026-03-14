using MessageBus.Abstractions;

namespace MessageBus.Handler
{
    public interface IMessageHandler<in TMessage> : IMessageHandler
        where TMessage : MessageBase
    {
        Task HandleAsync(TMessage message);
        Task IMessageHandler.HandleAsync(MessageBase message) => HandleAsync((TMessage)message);
    }

    public interface IMessageHandler
    {
        Task HandleAsync(MessageBase message);
    }
}
