using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Publisher
{
    public class RabbitMqMessageBus(ILogger<RabbitMqMessageBus> logger, IConnection connection, IHostApplicationLifetime applicationLifetime)
        : IHostedService, IMessagePublisher, IDisposable
    {
        private const string ExchangeName = "ReviewsPlatformMB";
        private IChannel? consumerChannel;

        public async Task PublishAsync(MessageBase messageBusEvent)
        {
            string routingKey = messageBusEvent.GetType().Name;

            await using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct);

            byte[] encodedMessage = JsonSerializer.SerializeToUtf8Bytes(messageBusEvent);
            var basicProperties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };
            await channel.BasicPublishAsync(exchange: ExchangeName, routingKey: routingKey, body: encodedMessage, 
                mandatory: true, basicProperties: basicProperties);

            logger.LogInformation("{MessageId} ({MessageName}) was successfully published", messageBusEvent.Id, routingKey);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!connection.IsOpen)
                    {
                        logger.LogCritical("RabbitMQ connection is not opened");
                        applicationLifetime.StopApplication();
                    }

                    consumerChannel = await connection.CreateChannelAsync(cancellationToken: CancellationToken.None);

                    await consumerChannel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, cancellationToken: CancellationToken.None);

                    var queueDeclareResult = await consumerChannel.QueueDeclareAsync(durable: true, exclusive: false, autoDelete: false,
                        arguments: null, cancellationToken: CancellationToken.None);
                    string queueName = queueDeclareResult.QueueName;

                    var consumer = new AsyncEventingBasicConsumer(consumerChannel);
                    consumer.ReceivedAsync += async (sender, eventArgs) =>
                    {


                        await consumerChannel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                    };

                    await consumerChannel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, 
                        cancellationToken: CancellationToken.None);

                    //bind queues
                }
                catch(Exception e)
                {
                    logger.LogCritical("RabbitMQ could not started. Exception message: {ExceptionMessage}", e.Message);
                    applicationLifetime.StopApplication();
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            consumerChannel?.Dispose();
        }
    }
}
