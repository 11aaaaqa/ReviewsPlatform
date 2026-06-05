using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Review;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewCreatedWithItemRejectedEventConsumer(IUnitOfWork unitOfWork, ILogger<ReviewCreatedWithItemRejectedEventConsumer> logger) 
        : IMessageHandler<ReviewCreatedWithItemRejectedEvent>
    {
        public async Task HandleAsync(ReviewCreatedWithItemRejectedEvent message)
        {
            try
            {
                await unitOfWork.ItemRepository.RemoveAsync(message.ItemId);
                await unitOfWork.CompleteAsync();
            }
            catch (ArgumentException e)
            {
                logger.LogWarning(e, "User added review using API endpoint with non-existent item identifier. Review was rejected manually and no item was removed");
            }
        }
    }
}
