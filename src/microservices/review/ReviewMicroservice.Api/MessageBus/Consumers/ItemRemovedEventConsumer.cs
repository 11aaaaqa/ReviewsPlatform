using MessageBus.Handler;
using MessageBus.Messages.Item;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class ItemRemovedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ItemRemovedEvent>
    {
        public async Task HandleAsync(ItemRemovedEvent message)
        {
            var reviewsToRemove = await unitOfWork.ReviewRepository.GetByItemIdAsync(message.ItemId);
            unitOfWork.ReviewRepository.RemoveRange(reviewsToRemove);
            await unitOfWork.CompleteAsync();
        }
    }
}
