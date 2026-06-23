using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public interface ICommentReplyRepository
    {
        Task<int> CountRepliesAsync(Guid commentId);
        Task AddAsync(CommentReply model);
        Task RemoveAsync(Guid parentCommentId, Guid repliedCommentId);
        void RemoveRange(List<CommentReply> replies);
    }
}