using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Review;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewsCreatedWithItemRejectedEventConsumer(IUnitOfWork unitOfWork)
        : IMessageHandler<ReviewsCreatedWithItemRejectedEvent>
    {
        public async Task HandleAsync(ReviewsCreatedWithItemRejectedEvent message)
        {
            var items = await unitOfWork.ItemRepository.GetByItemIdsAsync(message.ItemIds);
            foreach (var item in items)
            {
                item.Status = ItemStatus.Rejected;
                unitOfWork.ItemRepository.Update(item);
            }
            await unitOfWork.CompleteAsync();
        }
    }
}
