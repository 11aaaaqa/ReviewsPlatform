using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices.ReactionServices.Repository
{
    public interface IReactionRepository
    {
        Task<int> CountReactionsAsync(Guid reviewId, ReactionType reactionType);
        Task<ReviewReaction?> GetByIdAsync(Guid userId, Guid reviewId);
        Task AddAsync(ReviewReaction reaction);
        Task RemoveAsync(Guid userId, Guid reviewId);
        void Update(ReviewReaction reaction);
    }
}
