using MessageBus.Handler;
using MessageBus.Messages.Saga.RejectReviewAndAddRestriction;
using MessageBus.Publisher;
using RestrictionMicroservice.Api.Enums;
using RestrictionMicroservice.Api.Models.Business;
using RestrictionMicroservice.Api.Services.UnitOfWork;

namespace RestrictionMicroservice.Api.MessageBus.Consumers
{
    public class ReviewRejectedSagaEventConsumer(IUnitOfWork unitOfWork, ILogger<ReviewRejectedSagaEventConsumer> logger,
        IMessagePublisher messagePublisher) : IMessageHandler<ReviewRejectedSagaEvent>
    {
        public async Task HandleAsync(ReviewRejectedSagaEvent message)
        {
            var activeRestriction = await unitOfWork.RestrictionRepository.GetActiveRestrictionByRestrictedUserIdAsync(message.RestrictedUserId);
            if (activeRestriction != null)
            {
                await messagePublisher.PublishAsync(new UserFailedToBeRestrictedSagaEvent { ReviewId = message.ReviewId });
                return;
            }

            try
            {
                await unitOfWork.RestrictionRepository.AddAsync(new Restriction
                {
                    Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, IsPermanent = message.IsPermanent, Reason = message.Reason,
                    RestrictingUserId = message.RestrictingUserId, RestrictedUserId = message.RestrictedUserId,
                    RestrictionType = (RestrictionType)message.RestrictionType,
                    ExpiryTime = message.IsPermanent ? DateTime.UtcNow : DateTime.UtcNow.Add(message.Duration),
                    IsDisabled = false, DisablingReason = null, DisabledAt = new DateTime(), DisabledByUserId = Guid.Empty
                });
                await unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "An exception was thrown while adding restriction");
                await messagePublisher.PublishAsync(new UserFailedToBeRestrictedSagaEvent { ReviewId = message.ReviewId });
                return;
            }

            await messagePublisher.PublishAsync(new UserRestrictedSagaEvent { ReviewId = message.ReviewId });
        }
    }
}
