using MessageBus.Abstractions;
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
        public static IMessageBusBuilder AddRabbitMqMessageBus(this IServiceCollection services, RabbitMqOptions rabbitMqOptions)
        {
            services.AddSingleton<IMessagePublisher>(sp => new RabbitMqMessageBus(
                sp.GetRequiredService<ILogger<RabbitMqMessageBus>>(),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<IOptions<MessageBusHandlerInfo>>(),
                sp.GetRequiredService<IServiceProvider>(),
                rabbitMqOptions));

            services.AddSingleton<IHostedService>(sp => (RabbitMqMessageBus)sp.GetRequiredService<IMessagePublisher>());

            return new MessageBusBuilder(services);
        }

        private class MessageBusBuilder(IServiceCollection services) : IMessageBusBuilder
        {
            public IServiceCollection Services => services;
        }
    }
}
