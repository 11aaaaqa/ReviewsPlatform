using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public class CommentReplyRepository(ApplicationDbContext context) : ICommentReplyRepository
    {
        public async Task<int> CountRepliesAsync(Guid commentId)
            => await context.CommentReplies.Where(x => x.ParentId == commentId).CountAsync();

        public async Task AddAsync(CommentReply model)
        {
            await context.CommentReplies.AddAsync(model);
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