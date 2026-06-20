using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.Services.CommentServices
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(Guid commentId);
        Task<List<Comment>> GetByReviewIdAsync(Guid reviewId, int pageSize, int pageNumber);
        Task<List<Comment>> GetByReplyToCommentIdAsync(Guid replyCommentId, int pageSize, int pageNumber);
        Task<List<Comment>> GetByUserIdAsync(Guid userId, int pageSize, int pageNumber);
        Task AddAsync(Comment model);
        void Update(Comment model);
        Task RemoveAsync(Guid commentId);
    }
}
