using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Saga.CreateItemWIthReview;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewFailedToCreateSagaEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ReviewFailedToCreateSagaEvent>
    {
        public async Task HandleAsync(ReviewFailedToCreateSagaEvent message)
        {
            await unitOfWork.ItemRepository.RemoveAsync(message.ItemId);
            await unitOfWork.CompleteAsync();
        }
    }
}
