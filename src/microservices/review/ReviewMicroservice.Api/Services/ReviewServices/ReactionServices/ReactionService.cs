using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.Services.ReviewServices.ReactionServices
{
    public class ReactionService(IUnitOfWork unitOfWork) : IReactionService
    {
        public async Task ReactAsync(Guid userId, Guid reviewId, ReactionType reactionType)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                var review = await unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
                if (review == null) throw new NotFoundException("Review with current identifier does not exist");
                if (review.UserId == userId) throw new SelfReactionNotAllowedException();

                var userReaction = await unitOfWork.ReactionRepository.GetByIdAsync(userId, reviewId);

                if (userReaction == null)
                {
                    await unitOfWork.ReactionRepository.AddAsync(new ReviewReaction
                       { ReactionType = reactionType, ReviewId = reviewId, UserId = userId });
                }
                else
                {
                    if (userReaction.ReactionType == reactionType)
                    {
                        await unitOfWork.ReactionRepository.RemoveAsync(userId, reviewId);
                    }
                    else
                    {
                        userReaction.ReactionType = reactionType;
                        unitOfWork.ReactionRepository.Update(userReaction);
                    }
                }

                await unitOfWork.CompleteAsync();

                review.LikesCount = await unitOfWork.ReactionRepository.CountReactionsAsync(review.Id, ReactionType.Like);
                review.DislikesCount = await unitOfWork.ReactionRepository.CountReactionsAsync(review.Id, ReactionType.Dislike);
                unitOfWork.ReviewRepository.Update(review);

                await unitOfWork.CompleteAsync();

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
