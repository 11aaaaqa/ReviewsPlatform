using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices
{
    public class CommentRepository(ApplicationDbContext context) : ICommentRepository
    {
        public async Task<Comment?> GetByIdAsync(Guid commentId)
            => await context.Comments.SingleOrDefaultAsync(x => x.Id == commentId);

        public async Task<List<Comment>> GetByReviewIdAsync(Guid reviewId, int pageSize, int pageNumber)
        {
            return await context.Comments
                .Where(x => x.ReviewId == reviewId)
                .Where(x => x.ReplyToCommentId == null)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetByReplyToCommentIdAsync(Guid replyCommentId, int pageSize, int pageNumber)
        {
            return await context.Comments
                .Where(x => x.ReplyToCommentId == replyCommentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();
        }

        public async Task<List<Comment>> GetByUserIdAsync(Guid userId, int pageSize, int pageNumber)
        {
            return await context.Comments
                .Where(x => x.UserId == userId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Comment model)
        {
            await context.Comments.AddAsync(model);
        }

        public void Update(Comment model)
        {
            context.Comments.Update(model);
        }

        public async Task RemoveAsync(Guid commentId)
        {
            var comment = await context.Comments.SingleOrDefaultAsync(x => x.Id == commentId);
            if (comment == null)
                throw new NotFoundException("Comment with current identifier does not exist");

            context.Comments.Remove(comment);
        }
    }
}
