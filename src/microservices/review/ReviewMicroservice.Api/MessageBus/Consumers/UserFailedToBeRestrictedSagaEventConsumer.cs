using MessageBus.Handler;
using MessageBus.Messages.Saga.RejectReviewAndAddRestriction;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class UserFailedToBeRestrictedSagaEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<UserFailedToBeRestrictedSagaEvent>
    {
        public async Task HandleAsync(UserFailedToBeRestrictedSagaEvent message)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(message.ReviewId);

            review!.ReviewStatus = EntityStatus.UnderConsideration;
            review.RejectionReason = null;
            await unitOfWork.CompleteAsync();
        }
    }
}
