using MessageBus.Handler;
using MessageBus.Messages.Saga.RejectCommentAndAddRestriction;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class CommentUserFailedToBeRestrictedSagaEventConsumer(IUnitOfWork unitOfWork, 
        ILogger<CommentUserFailedToBeRestrictedSagaEventConsumer> logger) : IMessageHandler<CommentUserFailedToBeRestrictedSagaEvent>
    {
        public async Task HandleAsync(CommentUserFailedToBeRestrictedSagaEvent message)
        {
            var comment = await unitOfWork.CommentRepository.GetByIdAsync(message.CommentId);

            comment!.CommentStatus = EntityStatus.UnderConsideration;
            comment.RejectionReason = null;
            comment.ConsideredByUserId = null;
            unitOfWork.CommentRepository.Update(comment);
            await unitOfWork.CompleteAsync();

            logger.LogError("SAGA failed to execute");
        }
    }
}
