using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices.CommentReplyServices
{
    public interface ICommentReplyRepository
    {
        Task<List<Guid>> GetCommentAncestorIdsAsync(Guid commentId);
        Task<List<Guid>> GetCommentDescendantIdsAsync(Guid commentId);
        Task ExecuteDeleteAllRelationshipsByIdsAsync(List<Guid> commentIds);
        Task AddAsync(CommentReply model);
        Task AddRangeAsync(List<CommentReply> commentReplies);
        Task RemoveAsync(Guid parentCommentId, Guid repliedCommentId);
        void RemoveRange(List<CommentReply> replies);
    }
}