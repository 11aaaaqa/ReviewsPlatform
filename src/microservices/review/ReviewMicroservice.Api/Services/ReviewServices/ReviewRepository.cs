using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Services.ReviewServices
{
    public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
    {
        public async Task<Review?> GetByIdAsync(Guid id)
            => await context.Reviews.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<ReviewNoPictures>> GetAllByStatusAsync(ReviewStatus status, OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ReviewStatus == status).Select(x => 
                new ReviewNoPictures
                {
                    Id = x.Id, ReviewStatus = x.ReviewStatus, UserId = x.UserId, ItemId = x.ItemId, ItemEstimation = x.ItemEstimation,
                    IsCreatedWithItem = x.IsCreatedWithItem, CreatedAt = x.CreatedAt, DislikesCount = x.DislikesCount,
                    LikesCount = x.LikesCount, RejectionReason = x.RejectionReason, ShortReview = x.ShortReview, Text = x.Text
                });
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

        public async Task<List<ReviewNoPictures>> GetByUserIdAsync(Guid userId, ReviewStatus reviewStatus,
            OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.UserId == userId)
                .Where(x => x.ReviewStatus == reviewStatus)
                .Select(x => 
                new ReviewNoPictures
                {
                    Id = x.Id, ReviewStatus = x.ReviewStatus, UserId = x.UserId, ItemId = x.ItemId, ItemEstimation = x.ItemEstimation,
                    IsCreatedWithItem = x.IsCreatedWithItem, CreatedAt = x.CreatedAt, DislikesCount = x.DislikesCount,
                    LikesCount = x.LikesCount, RejectionReason = x.RejectionReason, ShortReview = x.ShortReview, Text = x.Text
                });
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

        public async Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, ReviewStatus reviewStatus, OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ItemId == itemId)
                .Where(x => x.ReviewStatus == reviewStatus)
                .Select(x => 
                new ReviewNoPictures
                {
                    Id = x.Id, ReviewStatus = x.ReviewStatus, UserId = x.UserId, ItemId = x.ItemId, ItemEstimation = x.ItemEstimation,
                    IsCreatedWithItem = x.IsCreatedWithItem, CreatedAt = x.CreatedAt, DislikesCount = x.DislikesCount,
                    LikesCount = x.LikesCount, RejectionReason = x.RejectionReason, ShortReview = x.ShortReview, Text = x.Text
                });
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

        public async Task<List<Review>> GetByItemIdAsync(Guid itemId)
            => await context.Reviews.Where(x => x.ItemId == itemId).ToListAsync();

        public async Task<List<ReviewNoPictures>> GetByItemIdByActualityAsync(Guid itemId, int pageNumber, int pageSize)
        {
            return await context.Reviews
                .Where(x => x.ItemId == itemId && x.ReviewStatus == ReviewStatus.Verified)
                .Select(x => 
                    new ReviewNoPictures
                    {
                        Id = x.Id, ReviewStatus = x.ReviewStatus, UserId = x.UserId, ItemId = x.ItemId, ItemEstimation = x.ItemEstimation,
                        IsCreatedWithItem = x.IsCreatedWithItem, CreatedAt = x.CreatedAt, DislikesCount = x.DislikesCount,
                        LikesCount = x.LikesCount, RejectionReason = x.RejectionReason, ShortReview = x.ShortReview, Text = x.Text
                    })
                .OrderByDescending(x => x.LikesCount)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();
        }

        public async Task<List<Review>> GetByItemIdAsync(List<Guid> itemIds)
            => await context.Reviews.Where(x => itemIds.Contains(x.ItemId)).ToListAsync();

        public async Task<List<ReviewNoPictures>> GetByItemIdAsync(Guid itemId, OrderByEstimation orderByEstimation, int pageNumber, int pageSize)
        {
            var reviews = context.Reviews.Where(x => x.ItemId == itemId && x.ReviewStatus == ReviewStatus.Verified)
                .Select(x => 
                    new ReviewNoPictures
                    {
                        Id = x.Id, ReviewStatus = x.ReviewStatus, UserId = x.UserId, ItemId = x.ItemId, ItemEstimation = x.ItemEstimation,
                        IsCreatedWithItem = x.IsCreatedWithItem, CreatedAt = x.CreatedAt, DislikesCount = x.DislikesCount,
                        LikesCount = x.LikesCount, RejectionReason = x.RejectionReason, ShortReview = x.ShortReview, Text = x.Text
                    });
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

        public void RemoveRange(List<Review> reviews)
        {
            context.Reviews.RemoveRange(reviews);
        }
    }
}
