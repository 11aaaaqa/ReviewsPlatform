using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Review;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewRemovedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ReviewRemovedEvent>
    {
        public async Task HandleAsync(ReviewRemovedEvent message)
        {
            var item = await unitOfWork.ItemRepository.GetByIdAsync(message.ItemId);
            if (message.IsReviewVerified)
            {
                var subcategory = await unitOfWork.SubcategoryRepository.GetByIdAsync(item!.SubcategoryId);
                var category = await unitOfWork.CategoryRepository.GetByIdAsync(subcategory!.CategoryId);

                double newEstimation = (item.GeneralEstimation * item.ReviewsCount - message.ItemEstimation) / (item.ReviewsCount - 1);
                item.GeneralEstimation = double.Round(newEstimation, 1);
                item.ReviewsCount--;
                unitOfWork.ItemRepository.Update(item);

                subcategory.ReviewsCount--;
                unitOfWork.SubcategoryRepository.Update(subcategory);

                category!.ReviewsCount--;
                unitOfWork.CategoryRepository.Update(category);

                await unitOfWork.CompleteAsync();
            }
            else
            {
                if (item!.ReviewsCount == 0)
                {
                    await unitOfWork.ItemRepository.RemoveAsync(item.Id);
                    await unitOfWork.CompleteAsync();
                }
            }
        }
    }
}
