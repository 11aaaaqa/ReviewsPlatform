using ReviewMicroservice.Api.Enums;

namespace ReviewMicroservice.Api.Services.ReviewServices.ReactionServices
{
    public interface IReactionService
    {
        Task ReactAsync(Guid userId, Guid reviewId, ReactionType reactionType);
    }
}
