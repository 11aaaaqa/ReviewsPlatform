using MessageBus.Handler;
using MessageBus.Messages.Category;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class SubcategoryRemovedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<SubcategoryRemovedEvent>
    {
        public async Task HandleAsync(SubcategoryRemovedEvent message)
        {
            var reviewsToDelete = await unitOfWork.ReviewRepository.GetByItemIdAsync(message.ItemIdsOfRemovedSubcategory);
            unitOfWork.ReviewRepository.RemoveRange(reviewsToDelete);
            await unitOfWork.CompleteAsync();
        }
    }
}
