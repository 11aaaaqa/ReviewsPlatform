using MessageBus.Handler;
using MessageBus.Messages.Category;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class CategoryRemovedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<CategoryRemovedEvent>
    {
        public async Task HandleAsync(CategoryRemovedEvent message)
        {
            var reviewsToDelete = await unitOfWork.ReviewRepository.GetByItemIdAsync(message.ItemIdsOfRemovedCategory);
            unitOfWork.ReviewRepository.RemoveRange(reviewsToDelete);
            await unitOfWork.CompleteAsync();
        }
    }
}
