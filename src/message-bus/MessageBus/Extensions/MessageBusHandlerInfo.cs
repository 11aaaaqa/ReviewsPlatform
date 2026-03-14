namespace MessageBus.Extensions
{
    public class MessageBusHandlerInfo
    {
        public Dictionary<string, Type> MessageTypes { get; } = new();
    }
}
