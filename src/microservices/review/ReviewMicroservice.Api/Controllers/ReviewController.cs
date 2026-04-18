using System.Security.Claims;
using MessageBus.Messages.Review;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewMicroservice.Api.Constants;
using ReviewMicroservice.Api.DTOs;
using ReviewMicroservice.Api.DTOs.review;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Models.Business;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher, ILogger<ReviewController> logger)
        : ControllerBase
    {
        [HttpGet]
        [Route("get-by-id/{reviewId}")]
        public async Task<IActionResult> GetReviewByIdAsync([FromRoute] Guid reviewId)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return NotFound("Reviews with current identifier does not exist");

            return Ok(review);
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpGet]
        [Route("get-under-consideration")]
        public async Task<IActionResult> GetAllReviewsUnderConsiderationAsync(int pageNumber, int pageSize)
        {
            var reviews = await unitOfWork.ReviewRepository.GetAllByStatusAsync(ReviewStatus.UnderConsideration,
                OrderByDate.Descending, pageNumber, pageSize);

            var reviewsNextPage = await unitOfWork.ReviewRepository.GetAllByStatusAsync(ReviewStatus.UnderConsideration,
                OrderByDate.Descending, pageNumber + 1, pageSize);

            return Ok(new ReviewsResult { IsNextPageExisted = reviewsNextPage.Count > 0, Reviews = reviews });
        }

        [HttpGet]
        [Route("get-by-user-id/{userId}")]
        public async Task<IActionResult> GetAllReviewsByUserIdAsync([FromRoute] Guid userId, ReviewStatus reviewStatus,
            OrderByDate orderByDate, int pageNumber, int pageSize)
        {
            var reviews =
                await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, reviewStatus, orderByDate, pageNumber, pageSize);

            var reviewsNextPage =
                await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, reviewStatus, orderByDate, pageNumber + 1, pageSize);

            return Ok(new ReviewsResult { Reviews = reviews, IsNextPageExisted = reviewsNextPage.Count > 0 });
        }

        [HttpGet]
        [Route("get-by-item-id/{itemId}")]
        public async Task<IActionResult> GetAllReviewsByItemIdAsync([FromRoute] Guid itemId, OrderByDate orderByDate,
            int pageNumber, int pageSize)
        {
            var reviews = await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, ReviewStatus.Verified, orderByDate,
                pageNumber, pageSize);

            var reviewsNextPage = await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, ReviewStatus.Verified, orderByDate,
                pageNumber + 1, pageSize);

            return Ok(new ReviewsResult { IsNextPageExisted = reviewsNextPage.Count > 0, Reviews = reviews });
        }

        [HttpGet]
        [Route("get-by-item-id-by-estimation/{itemId}")]
        public async Task<IActionResult> GetAllReviewsByItemIdByEstimationAsync([FromRoute] Guid itemId, OrderByEstimation orderByEstimation,
            int pageNumber, int pageSize)
        {
            var reviews =
                await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, orderByEstimation, pageNumber, pageSize);

            var reviewsNextPage =
                await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, orderByEstimation, pageNumber + 1, pageSize);

            return Ok(new ReviewsResult { Reviews = reviews, IsNextPageExisted = reviewsNextPage.Count > 0 });
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateReviewAsync([FromBody] UpdateReviewDto model)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(model.Id);
            if (review == null)
                return NotFound("Review with current identifier does not exist");

            if (review.ReviewStatus != ReviewStatus.UnderConsideration)
                return BadRequest("Review must be in under consideration status");

            string oldShortReview = review.ShortReview;

            review.ShortReview = model.ShortReview;
            unitOfWork.ReviewRepository.Update(review);
            await unitOfWork.CompleteAsync();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} updated short review from {OldShortReview} to {NewShortReview}",
                userIdStr, oldShortReview, model.ShortReview);

            return Ok();
        }

        [Authorize]
        [HttpDelete]
        [Route("remove/{reviewId}")]
        public async Task<IActionResult> RemoveReviewAsync(Guid reviewId)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return NotFound("Review with current identifier does not exist");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            if (review.UserId != userId && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Moderator))
                return Forbid();

            Guid itemId = review.ItemId;
            ReviewStatus reviewStatus = review.ReviewStatus;
            int itemEstimation = review.ItemEstimation;

            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.ReviewRepository.RemoveAsync(reviewId);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ReviewRemovedEvent
                    { ItemId = itemId, IsReviewVerified = reviewStatus == ReviewStatus.Verified, ItemEstimation = itemEstimation});

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing review removing method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if(User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Moderator))
                logger.LogInformation("User {UserId} removed review {ReviewId}", userId, reviewId);

            return Ok();
        }

        [RequestSizeLimit(5 * 2 * 1024 * 1024)]
        [Authorize]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> SuggestReviewAsync([FromBody] AddReviewDto model)
        {
            if (model == null)
                return BadRequest("Request size exceeds the limit");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            //gRPC to check if user allowed to add

            List<byte[]> pictures = new();
            if (model.Pictures.Count > 0)
            {
                using MemoryStream memoryStream = new MemoryStream();
                foreach (var picture in model.Pictures)
                {
                    await picture.CopyToAsync(memoryStream);
                    pictures.Add(memoryStream.ToArray());
                }
            }

            var reviewToAdd = new Review
            {
                Id = Guid.NewGuid(), UserId = userId, ItemId = model.ItemId, CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                IsCreatedWithItem = false, DislikesCount = 0, LikesCount = 0, ItemEstimation = model.ItemEstimation,
                RejectionReason = null, ReviewStatus = ReviewStatus.UnderConsideration, ShortReview = model.ShortReview,
                Text = model.Text, Pictures = pictures
            };

            await unitOfWork.ReviewRepository.AddAsync(reviewToAdd);
            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpGet]
        [Route("accept-review/{reviewId}")]
        public async Task<IActionResult> AcceptReviewAsync([FromRoute] Guid reviewId)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return NotFound("Review with current identifier does not exist");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                review.ReviewStatus = ReviewStatus.Verified;
                unitOfWork.ReviewRepository.Update(review);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ReviewAcceptedEvent
                    { ItemId = review.ItemId, IsReviewCreatedWithItem = review.IsCreatedWithItem, ItemEstimation = review.ItemEstimation });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing review accepting method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} accepted review {ReviewId}", userIdStr, reviewId);

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpPut]
        [Route("reject-review")]
        public async Task<IActionResult> RejectReviewAsync([FromBody] RejectReviewDto model)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(model.ReviewId);
            if (review == null)
                return NotFound("Review with current identifier does not exist");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                review.ReviewStatus = ReviewStatus.Rejected;
                review.RejectionReason = model.RejectionReason;
                unitOfWork.ReviewRepository.Update(review);
                await unitOfWork.CompleteAsync();

                if (review.IsCreatedWithItem)
                    await messagePublisher.PublishAsync(new ReviewCreatedWithItemRejectedEvent { ItemId = review.ItemId });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing review rejecting method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            logger.LogInformation("User {UserId} rejected review {ReviewId}", userIdStr, model.ReviewId);

            return Ok();
        }
    }
}