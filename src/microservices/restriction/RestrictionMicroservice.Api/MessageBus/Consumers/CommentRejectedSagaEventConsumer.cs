using MessageBus.Handler;
using MessageBus.Messages.Saga.RejectCommentAndAddRestriction;
using MessageBus.Publisher;
using RestrictionMicroservice.Api.Enums;
using RestrictionMicroservice.Api.Models.Business;
using RestrictionMicroservice.Api.Services.UnitOfWork;

namespace RestrictionMicroservice.Api.MessageBus.Consumers
{
    public class CommentRejectedSagaEventConsumer(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher,
        ILogger<CommentRejectedSagaEventConsumer> logger) : IMessageHandler<CommentRejectedSagaEvent>
    {
        public async Task HandleAsync(CommentRejectedSagaEvent message)
        {
            var restriction = 
                await unitOfWork.RestrictionRepository.GetActiveRestrictionByRestrictedUserIdAsync(message.RestrictedUserId);
            if (restriction != null)
            {
                await messagePublisher.PublishAsync(new CommentUserFailedToBeRestrictedSagaEvent { CommentId = message.CommentId });
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
                await messagePublisher.PublishAsync(new CommentUserFailedToBeRestrictedSagaEvent { CommentId = message.CommentId });
                logger.LogCritical(e, "An exception was thrown while adding restriction");
            }
        }
    }
}
