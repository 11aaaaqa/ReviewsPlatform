using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public class CommentReplyRepository(ApplicationDbContext context) : ICommentReplyRepository
    {
        public async Task<List<CommentReply>> GetCommentAncestorsAsync(Guid commentId)
        {
            return await context.CommentReplies
                .Where(x => x.RepliedId == commentId)
                .Select(x => new CommentReply { ParentId = x.ParentId, RepliedId = commentId })
                .ToListAsync();
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