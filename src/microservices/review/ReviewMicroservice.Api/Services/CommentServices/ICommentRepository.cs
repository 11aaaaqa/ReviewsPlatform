using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(Guid commentId);
        Task<List<Comment>> GetByReviewIdAsync(Guid reviewId, int pageSize, int pageNumber);
        Task<List<Comment>> GetCommentRepliesAsync(Guid parentCommentId, int pageSize, int pageNumber);
        Task<List<Comment>> GetByUserIdAsync(Guid userId, int pageSize, int pageNumber);
        Task<List<Guid>> GetAllCommentIdsByReviewIdAsync(Guid reviewId);
        Task ExecuteRepliesCountUpdateAsync(List<Guid> commentIds, int delta);
        Task ExecuteDeleteCommentsByIdsAsync(List<Guid> commentIds);
        Task ExecuteDeleteCommentsByParentIdsAsync(List<Guid> parentIds);
        Task ExecuteDeleteCommentsByReviewIdAsync(Guid reviewId);
        Task AddAsync(Comment model);
        void Update(Comment model);
        Task RemoveAsync(Guid commentId);
    }
}
