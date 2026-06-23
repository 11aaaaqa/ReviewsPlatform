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
                .Where(x => x.ParentCommentId == null)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetCommentRepliesAsync(Guid parentCommentId, int pageNumber, int pageSize)
        {
            return await context.Comments
                .Where(x => x.ParentCommentId == parentCommentId)
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetByUserIdAsync(Guid userId, int pageSize, int pageNumber)
        {
            return await context.Comments
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
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
