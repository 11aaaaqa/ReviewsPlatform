using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<List<ReviewNoPictures>> GetAllByStatusAsync(ReviewStatus status, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByUserIdAsync(Guid userId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, OrderByEstimation orderByEstimation, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdAsync(Guid itemId);
        Task<List<ReviewNoPictures>> GetByItemIdByActualityAsync(Guid itemId, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdAsync(List<Guid> itemIds);
        Task ExecuteCommentsCountUpdateAsync(Guid reviewId, int delta);
        Task AddAsync(Review review);
        void Update(Review review);
        Task RemoveAsync(Guid reviewId);
        void RemoveRange(List<Review> reviews);
    }
}
