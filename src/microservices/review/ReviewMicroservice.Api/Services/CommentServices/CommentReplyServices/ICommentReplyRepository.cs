using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public interface ICommentReplyRepository
    {
        Task<List<CommentReply>> GetCommentAncestorsAsync(Guid commentId);
        Task AddAsync(CommentReply model);
        Task AddRangeAsync(List<CommentReply> commentReplies);
        Task RemoveAsync(Guid parentCommentId, Guid repliedCommentId);
        void RemoveRange(List<CommentReply> replies);
    }
}