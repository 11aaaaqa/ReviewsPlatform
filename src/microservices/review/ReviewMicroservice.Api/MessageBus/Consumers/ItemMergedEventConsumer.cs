using MessageBus.Handler;
using MessageBus.Messages.Category;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class ItemMergedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ItemMergedEvent>
    {
        public async Task HandleAsync(ItemMergedEvent message)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(message.ReviewId);
            review!.ItemId = message.MergedItemId;
            review.IsCreatedWithItem = false;
            await unitOfWork.CompleteAsync();
        }
    }
}
