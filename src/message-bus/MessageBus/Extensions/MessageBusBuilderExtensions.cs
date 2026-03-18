using MessageBus.Abstractions;
using MessageBus.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Extensions
{
    public static class MessageBusBuilderExtensions
    {
        public static IMessageBusBuilder AddMessageBusHandler<TMessage, THandler>(this IMessageBusBuilder builder)
            where TMessage : MessageBase
            where THandler : class, IMessageHandler<TMessage>
        {
            builder.Services.Configure<MessageBusHandlerInfo>(handlerInfo =>
            {
                handlerInfo.MessageTypes[typeof(TMessage).Name] = typeof(TMessage);
            });

            builder.Services.AddKeyedTransient<IMessageHandler, THandler>(typeof(TMessage));

            return builder;
        }
    }
}
