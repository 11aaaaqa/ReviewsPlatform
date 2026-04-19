using MessageBus.Handler;
using MessageBus.Messages.Review;
using RestrictionMicroservice.Api.Services.UnitOfWork;

namespace RestrictionMicroservice.Api.MessageBus.Consumers
{
    public class ReviewRemovedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ReviewRemovedEvent>
    {
        public async Task HandleAsync(ReviewRemovedEvent message)
        {
            await unitOfWork.ReportRepository.RemoveAllByReportOnEntityIdAsync(message.ItemId);
            await unitOfWork.CompleteAsync();
        }
    }
}
