using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Abstractions
{
    public interface IMessageBusBuilder
    {
        public IServiceCollection Services { get; }
    }
}
