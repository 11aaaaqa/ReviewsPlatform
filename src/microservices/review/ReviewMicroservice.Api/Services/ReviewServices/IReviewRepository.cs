using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<List<Review>> GetByUserIdAsync(Guid userId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdAsync(Guid itemId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize);
        Task<List<Review>> GetByItemIdByEstimationAsync(Guid itemId, OrderByEstimation orderByEstimation, int pageNumber, int pageSize);
        Task AddAsync(Review review);
        void Update(Review review);
        Task RemoveAsync(Guid reviewId);
    }
}
