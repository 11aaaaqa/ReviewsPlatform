using CategoryMicroservice.Api.Enums;
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
            var item = await unitOfWork.ItemRepository.GetByIdAsync(message.ItemId);
            item!.Status = ItemStatus.Rejected;
            await unitOfWork.CompleteAsync();
        }
    }
}
