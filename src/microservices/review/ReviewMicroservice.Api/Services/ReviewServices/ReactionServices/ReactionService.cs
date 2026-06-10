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

                    switch (reactionType)
                    {
                        case ReactionType.Like:
                            review.LikesCount++;
                            break;
                        case ReactionType.Dislike:
                            review.DislikesCount++;
                            break;
                        default: throw new ArgumentOutOfRangeException(nameof(reactionType));
                    }
                }
                else
                {
                    if (userReaction.ReactionType == reactionType)
                    {
                        await unitOfWork.ReactionRepository.RemoveAsync(userId, reviewId);

                        switch (reactionType)
                        {
                            case ReactionType.Like:
                                review.LikesCount--;
                                break;
                            case ReactionType.Dislike:
                                review.DislikesCount--;
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(reactionType));
                        }
                    }
                    else
                    {
                        userReaction.ReactionType = reactionType;
                        unitOfWork.ReactionRepository.Update(userReaction);

                        switch (reactionType)
                        {
                            case ReactionType.Like:
                                review.LikesCount++;
                                review.DislikesCount--;
                                break;
                            case ReactionType.Dislike:
                                review.DislikesCount++;
                                review.LikesCount--;
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(reactionType));
                        }
                    }
                }

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
