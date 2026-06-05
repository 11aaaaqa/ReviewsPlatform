using MessageBus.Handler;
using MessageBus.Messages.Saga.CreateItemWIthReview;
using MessageBus.Publisher;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.MessageBus.Consumers
{
    public class ItemCreatedSagaEventConsumer(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher,
        ILogger<ItemCreatedSagaEventConsumer> logger) : IMessageHandler<ItemCreatedSagaEvent>
    {
        public async Task HandleAsync(ItemCreatedSagaEvent message)
        {
            var review = new Review
            {
                Id = Guid.NewGuid(), UserId = message.UserIdCreatedBy,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                DislikesCount = 0, ItemEstimation = message.ReviewItemEstimation, ItemId = message.ItemId, LikesCount = 0,
                Pictures = message.ReviewPictures,
                ReviewStatus = ReviewStatus.UnderConsideration, ShortReview = message.ShortReview,
                Text = message.ReviewText, IsCreatedWithItem = true, RejectionReason = null
            };

            try
            {
                await unitOfWork.ReviewRepository.AddAsync(review);
                await unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "An exception was thrown while creating review");
                await messagePublisher.PublishAsync(new ReviewFailedToCreateSagaEvent { ItemId = message.ItemId });
                return;
            }

            await messagePublisher.PublishAsync(new ReviewCreatedSagaEvent { ItemId = message.ItemId });
        }
    }
}
