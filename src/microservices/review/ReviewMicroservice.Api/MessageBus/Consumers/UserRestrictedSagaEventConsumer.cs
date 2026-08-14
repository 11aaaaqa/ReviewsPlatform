using MessageBus.Handler;
using MessageBus.Messages.Review;
using MessageBus.Messages.Saga.RejectReviewAndAddRestriction;
using MessageBus.Publisher;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class UserRestrictedSagaEventConsumer(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher,
        ILogger<UserRestrictedSagaEventConsumer> logger) : IMessageHandler<UserRestrictedSagaEvent>
    {
        public async Task HandleAsync(UserRestrictedSagaEvent message)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(message.ReviewId);

            if (review!.IsCreatedWithItem) 
                await messagePublisher.PublishAsync(new ReviewCreatedWithItemRejectedEvent { ItemId = review.ItemId });

            logger.LogInformation("SAGA has been successfully completed. Review has been rejected and user has been restricted.");
        }
    }
}
