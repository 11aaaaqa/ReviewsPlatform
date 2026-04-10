using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices
{
    public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
    {
        public async Task<Review?> GetByIdAsync(Guid id)
            => await context.Reviews.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Review>> GetAllByStatusAsync(ReviewStatus status, OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ReviewStatus == status);
            switch (orderByDate)
            {
                case OrderByDate.Descending:
                    reviews = reviews.OrderByDescending(x => x.CreatedAt);
                    break;
                case OrderByDate.Ascending:
                    reviews = reviews.OrderBy(x => x.CreatedAt);
                    break;
            }

            return await reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Review>> GetByUserIdAsync(Guid userId, ReviewStatus reviewStatus,
            OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.UserId == userId)
                .Where(x => x.ReviewStatus == reviewStatus);
            switch (orderByDate)
            {
                case OrderByDate.Ascending:
                    reviews = reviews.OrderBy(x => x.CreatedAt);
                    break;
                case OrderByDate.Descending:
                    reviews = reviews.OrderByDescending(x => x.CreatedAt);
                    break;
            }

            return await reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Review>> GetByItemIdAsync(Guid itemId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ItemId == itemId)
                .Where(x => x.ReviewStatus == reviewStatus);
            switch (orderByDate)
            {
                case OrderByDate.Ascending:
                    reviews = reviews.OrderBy(x => x.CreatedAt);
                    break;
                case OrderByDate.Descending:
                    reviews = reviews.OrderByDescending(x => x.CreatedAt);
                    break;
            }

            return await reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Review>> GetByItemIdByEstimationAsync(Guid itemId, OrderByEstimation orderByEstimation, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ItemId == itemId);
            switch (orderByEstimation)
            {
                case OrderByEstimation.Ascending:
                    reviews = reviews.OrderBy(x => x.ItemEstimation);
                    break;
                case OrderByEstimation.Descending:
                    reviews = reviews.OrderByDescending(x => x.ItemEstimation);
                    break;
            }

            return await reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task AddAsync(Review review)
        {
            await context.Reviews.AddAsync(review);
        }

        public void Update(Review review)
        {
            context.Reviews.Update(review);
        }

        public async Task RemoveAsync(Guid reviewId)
        {
            var review = await context.Reviews.SingleOrDefaultAsync(x => x.Id == reviewId);
            if (review == null)
                throw new ArgumentException("Review with current identifier does not exist");

            context.Reviews.Remove(review);
        }
    }
}
