using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Handler;
using MessageBus.Messages.Review;

namespace CategoryMicroservice.Api.MessageBus.Consumers
{
    public class ReviewAcceptedEventConsumer(IUnitOfWork unitOfWork) : IMessageHandler<ReviewAcceptedEvent>
    {
        public async Task HandleAsync(ReviewAcceptedEvent message)
        {
            var item = await unitOfWork.ItemRepository.GetByIdAsync(message.ItemId);
            Guid subcategoryId = item!.SubcategoryId;

            if (message.IsReviewCreatedWithItem)
            {
                var items = await unitOfWork.ItemRepository.GetByNameAsync(item.Name);
                var existingItem = items.SingleOrDefault(x => x.Status == ItemStatus.Verified && x.SubcategoryId == subcategoryId);
                if (existingItem != null)
                {
                    double newEstimation = (existingItem.GeneralEstimation * existingItem.ReviewsCount + message.ItemEstimation) /
                                           (existingItem.ReviewsCount + 1);
                    existingItem.GeneralEstimation = double.Round(newEstimation, 1);
                    existingItem.ReviewsCount++;
                    unitOfWork.ItemRepository.Update(existingItem);

                    await unitOfWork.ItemRepository.RemoveAsync(item.Id);
                }
                else
                {
                    item.Status = ItemStatus.Verified;
                    item.ReviewsCount = 1;
                    item.GeneralEstimation = message.ItemEstimation;
                    unitOfWork.ItemRepository.Update(item);
                }
            }
            else
            {
                double newEstimation = (item.GeneralEstimation * item.ReviewsCount + message.ItemEstimation) / (item.ReviewsCount + 1);
                item.GeneralEstimation = double.Round(newEstimation, 1);
                item.ReviewsCount++;
                unitOfWork.ItemRepository.Update(item);
            }

            var subcategory = await unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId);
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(subcategory!.CategoryId);

            subcategory.ReviewsCount++;
            unitOfWork.SubcategoryRepository.Update(subcategory);

            category!.ReviewsCount++;
            unitOfWork.CategoryRepository.Update(category);

            await unitOfWork.CompleteAsync();
        }
    }
}
