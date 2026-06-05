using System.Text;
using System.Text.Json;
using MessageBus.Abstractions;
using MessageBus.Extensions;
using MessageBus.Handler;
using MessageBus.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqMessageBus.Exceptions;
using RabbitMqMessageBus.Extensions;

namespace RabbitMqMessageBus
{
    public class RabbitMqMessageBus
        : IHostedService, IMessagePublisher, IDisposable
    {
        private const string ExchangeName = "ReviewsPlatformMB";
        private readonly ILogger<RabbitMqMessageBus> logger;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly RabbitMqOptions rabbitMqOptions;
        private readonly IOptions<MessageBusHandlerInfo> handlerInfo;
        private readonly IServiceProvider serviceProvider;
        private IConnection? connection;
        private IChannel? consumerChannel;

        public RabbitMqMessageBus(ILogger<RabbitMqMessageBus> logger, IHostApplicationLifetime applicationLifetime,
            IOptions<MessageBusHandlerInfo> handlerInfo, IServiceProvider serviceProvider, RabbitMqOptions rabbitMqOptions)
        {
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
            this.rabbitMqOptions = rabbitMqOptions;
            this.handlerInfo = handlerInfo;
            this.serviceProvider = serviceProvider;
        }

        public async Task PublishAsync(MessageBase messageBusEvent)
        {
            if (connection == null || !connection.IsOpen)
                throw new RabbitMqConnectionIsNotOpenedException();

            string routingKey = messageBusEvent.GetType().Name;

            await using var channel = await connection!.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct);

            byte[] encodedMessage = JsonSerializer.SerializeToUtf8Bytes(messageBusEvent, messageBusEvent.GetType());
            var basicProperties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };
            await channel.BasicPublishAsync(exchange: ExchangeName, routingKey: routingKey, body: encodedMessage,
                mandatory: true, basicProperties: basicProperties);

            logger.LogInformation("Message {MessageId} ({MessageName}) was successfully published", messageBusEvent.MessageId, routingKey);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    ConnectionFactory factory = new ConnectionFactory
                    {
                        UserName = rabbitMqOptions.UserName,
                        Password = rabbitMqOptions.Password,
                        VirtualHost = rabbitMqOptions.VirtualHost,
                        HostName = rabbitMqOptions.HostName
                    };

                    await Policy.Handle<Exception>().WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(3),
                        (_, _) => logger.LogError("Error connecting to RabbitMQ. Retrying in 3 seconds")).ExecuteAsync(async () =>
                    {
                        connection = await factory.CreateConnectionAsync(CancellationToken.None);
                    });

                    if (connection == null || !connection.IsOpen)
                    {
                        logger.LogCritical("RabbitMQ connection is not opened");
                        applicationLifetime.StopApplication();
                        return;
                    }

                    if (handlerInfo.Value.MessageTypes.Count > 0)
                    {
                        consumerChannel = await connection.CreateChannelAsync(cancellationToken: CancellationToken.None);

                        await consumerChannel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, cancellationToken: CancellationToken.None);

                        await consumerChannel.QueueDeclareAsync(queue: rabbitMqOptions.QueueName, durable: true, exclusive: false,
                            autoDelete: false, arguments: null, cancellationToken: CancellationToken.None);

                        foreach (var (messageName, _) in handlerInfo.Value.MessageTypes)
                        {
                            await consumerChannel.QueueBindAsync(queue: rabbitMqOptions.QueueName, exchange: ExchangeName,
                                routingKey: messageName, cancellationToken: CancellationToken.None);
                        }

                        var consumer = new AsyncEventingBasicConsumer(consumerChannel);
                        consumer.ReceivedAsync += OnMessageReceived;

                        await consumerChannel.BasicConsumeAsync(queue: rabbitMqOptions.QueueName, autoAck: false, consumer: consumer,
                            cancellationToken: CancellationToken.None);
                    }
                    logger.LogInformation("RabbitMQ is successfully started");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex,"RabbitMQ could not started");
                    applicationLifetime.StopApplication();
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            string messageName = eventArgs.RoutingKey;
            string serializedMessage = Encoding.UTF8.GetString(eventArgs.Body.Span);
            Type messageType = handlerInfo.Value.MessageTypes[messageName];

            var message = JsonSerializer.Deserialize(serializedMessage, messageType) as MessageBase;

            using var scope = serviceProvider.CreateScope();

            try
            {
                foreach (var handler in scope.ServiceProvider.GetKeyedServices<IMessageHandler>(messageType))
                {
                    await handler.HandleAsync(message!);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exception was thrown while handling message {RoutingKey}", eventArgs.RoutingKey);
                await consumerChannel!.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false, CancellationToken.None);

                throw;
            }
            
            await consumerChannel!.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
        }

        public void Dispose()
        {
            consumerChannel?.Dispose();
            connection?.Dispose();
        }
    }
}
