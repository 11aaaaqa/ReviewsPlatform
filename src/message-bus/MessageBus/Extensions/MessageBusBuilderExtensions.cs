using MessageBus.Abstractions;
using MessageBus.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageBus.Extensions
{
    public static class MessageBusBuilderExtensions
    {
        public static void AddMessageBusHandler<TMessage, THandler>(this IHostApplicationBuilder builder)
            where TMessage : MessageBase
            where THandler : class, IMessageHandler<TMessage>
        {
            builder.Services.Configure<MessageBusHandlerInfo>(handlerInfo =>
            {
                handlerInfo.MessageTypes[typeof(TMessage).Name] = typeof(TMessage);
            });

            builder.Services.AddKeyedTransient<IMessageHandler, THandler>(typeof(TMessage));
        }
    }
}
