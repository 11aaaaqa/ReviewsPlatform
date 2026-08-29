using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<Review?> GetByIdAsync(Guid id, EntityStatus status);
        Task<List<ReviewNoPictures>> GetAllByStatusAsync(EntityStatus status, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByUserIdAsync(Guid userId, EntityStatus status);
        Task<List<ReviewNoPictures>> GetByUserIdAsync(Guid userId, EntityStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, EntityStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, OrderByEstimation orderByEstimation, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdAsync(Guid itemId);
        Task<List<ReviewNoPictures>> GetByItemIdByActualityAsync(Guid itemId, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdAsync(List<Guid> itemIds);
        Task ExecuteCommentsCountUpdateAsync(Guid reviewId, int delta);
        Task ExecuteReviewsUpdateAsync(List<Guid> reviewIds, EntityStatus newStatus, string rejectionReason);
        Task AddAsync(Review review);
        void Update(Review review);
        Task RemoveAsync(Guid reviewId);
        void RemoveRange(List<Review> reviews);
    }
}
