using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public class CommentReplyRepository(ApplicationDbContext context) : ICommentReplyRepository
    {
        public async Task<List<Guid>> GetCommentAncestorIdsAsync(Guid commentId)
            => await context.CommentReplies.Where(x => x.RepliedId == commentId).Select(x => x.ParentId).ToListAsync();

        public async Task<List<Guid>> GetCommentDescendantIdsAsync(Guid commentId)
            => await context.CommentReplies.Where(x => x.ParentId == commentId).Select(x => x.RepliedId).ToListAsync();

        public async Task ExecuteDeleteAllRelationshipsByIdsAsync(List<Guid> commentIds)
        {
            await context.CommentReplies
                .Where(x => commentIds.Contains(x.ParentId) || commentIds.Contains(x.RepliedId))
                .ExecuteDeleteAsync();
        }

        public async Task AddAsync(CommentReply model)
        {
            await context.CommentReplies.AddAsync(model);
        }

        public async Task AddRangeAsync(List<CommentReply> commentReplies)
        {
            await context.CommentReplies.AddRangeAsync(commentReplies);
        }

        public async Task RemoveAsync(Guid parentCommentId, Guid repliedCommentId)
        {
            var commentReply = await context.CommentReplies.SingleOrDefaultAsync(x 
                => x.ParentId == parentCommentId && x.RepliedId == repliedCommentId);
            if (commentReply == null)
                throw new NotFoundException("Reply does not exist");

            context.CommentReplies.Remove(commentReply);
        }

        public void RemoveRange(List<CommentReply> replies)
        {
            context.CommentReplies.RemoveRange(replies);
        }
    }
}