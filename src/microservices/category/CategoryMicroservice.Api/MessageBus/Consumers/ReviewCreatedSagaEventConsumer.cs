using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Saga.CreateItemWIthReview;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewCreatedSagaEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ReviewCreatedSagaEvent>
    {
        public async Task HandleAsync(ReviewCreatedSagaEvent message)
        {
            var review = await unitOfWork.ItemRepository.GetByIdAsync(message.ItemId);
            review!.Status = ItemStatus.UnderConsideration;
            unitOfWork.ItemRepository.Update(review);
            await unitOfWork.CompleteAsync();
        }
    }
}
