using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices.ReactionServices.Repository
{
    public class ReactionRepository(ApplicationDbContext context) : IReactionRepository
    {
        public async Task<int> CountReactionsAsync(Guid reviewId, ReactionType reactionType)
            => await context.ReviewReactions
                .Where(x => x.ReviewId == reviewId)
                .Where(x => x.ReactionType == reactionType)
                .CountAsync();

        public async Task<ReviewReaction?> GetByIdAsync(Guid userId, Guid reviewId)
            => await context.ReviewReactions.SingleOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId);

        public async Task AddAsync(ReviewReaction reaction)
        {
            await context.ReviewReactions.AddAsync(reaction);
        }

        public async Task RemoveAsync(Guid userId, Guid reviewId)
        {
            var reaction = 
                await context.ReviewReactions.SingleOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId);
            if (reaction == null)
                throw new ArgumentException("Reaction with current identifier does not exist");

            context.ReviewReactions.Remove(reaction);
        }

        public void Update(ReviewReaction reaction)
        {
            context.ReviewReactions.Update(reaction);
        }
    }
}