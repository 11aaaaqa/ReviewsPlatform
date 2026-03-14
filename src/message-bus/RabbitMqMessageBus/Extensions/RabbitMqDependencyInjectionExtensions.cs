using MessageBus.Extensions;
using MessageBus.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RabbitMqMessageBus.Extensions
{
    public static class RabbitMqDependencyInjectionExtensions
    {
        public static void AddRabbitMqMessageBus(this IHostApplicationBuilder builder, RabbitMqOptions rabbitMqOptions)
        {
            builder.Services.AddSingleton<IMessagePublisher>(sp => new RabbitMqMessageBus(
                sp.GetRequiredService<ILogger<RabbitMqMessageBus>>(),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<IOptions<MessageBusHandlerInfo>>(),
                sp.GetRequiredService<IServiceProvider>(),
                rabbitMqOptions));

            builder.Services.AddHostedService<IHostedService>(sp => (RabbitMqMessageBus)sp.GetRequiredService<IMessagePublisher>());
        }
    }
}
