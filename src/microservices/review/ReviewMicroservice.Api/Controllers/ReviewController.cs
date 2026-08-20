using MessageBus.Messages.Review;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewMicroservice.Api.Constants;
using ReviewMicroservice.Api.DTOs;
using ReviewMicroservice.Api.DTOs.review;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models;
using ReviewMicroservice.Api.Models.Business;
using ReviewMicroservice.Api.Services;
using ReviewMicroservice.Api.Services.ReviewServices.ReactionServices;
using ReviewMicroservice.Api.Services.UnitOfWork;
using System.Security.Claims;
using MessageBus.Messages.Saga.RejectReviewAndAddRestriction;
using ReviewMicroservice.Api.Filters.AuthorizationFilters;

namespace ReviewMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher, ILogger<ReviewController> logger,
        ImageValidator imageValidator, IReactionService reactionService) : ControllerBase
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

        [Authorize(Roles = RoleNames.Verified)]
        [HttpGet]
        [Route("get-review-reaction/{reviewId}")]
        public async Task<IActionResult> GetReviewReactionAsync([FromRoute] Guid reviewId)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            var reaction = await unitOfWork.ReactionRepository.GetByIdAsync(userId, reviewId);
            if (reaction == null) return NotFound();

            return Ok(reaction);
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpGet]
        [Route("get-under-consideration")]
        public async Task<IActionResult> GetAllReviewsUnderConsiderationAsync([FromQuery] Pagination pagination)
        {
            var reviews = await unitOfWork.ReviewRepository.GetAllByStatusAsync(EntityStatus.UnderConsideration,
                OrderByDate.Ascending, pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage = await unitOfWork.ReviewRepository.GetAllByStatusAsync(EntityStatus.UnderConsideration,
                OrderByDate.Ascending, pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { IsNextPageExisted = reviewsNextPage.Count > 0, Reviews = reviews });
        }

        [HttpGet]
        [Route("get/all/{userId}/verified")]
        public async Task<IActionResult> GetAllVerifiedReviewsByUserIdAsync([FromRoute] Guid userId, [FromQuery] Pagination pagination)
        {
            var reviews = await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, EntityStatus.Verified,
                OrderByDate.Descending, pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage = await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, EntityStatus.Verified,
                OrderByDate.Descending, pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { Reviews = reviews, IsNextPageExisted = reviewsNextPage.Count > 0 });
        }

        [Authorize]
        [HttpGet]
        [Route("get/all/{userId}")]
        public async Task<IActionResult> GetAllReviewsByUserIdAsync([FromRoute] Guid userId, [FromQuery] EntityStatus reviewStatus,
            [FromQuery] OrderByDate orderByDate, [FromQuery] Pagination pagination)
        {
            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid currentUserId = new Guid(currentUserIdStr);
            if (currentUserId != userId && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Moderator)) return Forbid();

            var reviews =
                await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, reviewStatus, orderByDate, pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage =
                await unitOfWork.ReviewRepository.GetByUserIdAsync(userId, reviewStatus, orderByDate, pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { Reviews = reviews, IsNextPageExisted = reviewsNextPage.Count > 0 });
        }

        [HttpGet]
        [Route("get-by-item-id/{itemId}")]
        public async Task<IActionResult> GetAllReviewsByItemIdAsync([FromRoute] Guid itemId, [FromQuery] OrderByDate orderByDate,
            [FromQuery] Pagination pagination)
        {
            var reviews = await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, EntityStatus.Verified, orderByDate,
                pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage = await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, EntityStatus.Verified, orderByDate,
                pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { IsNextPageExisted = reviewsNextPage.Count > 0, Reviews = reviews });
        }

        [HttpGet]
        [Route("get-by-item-id-by-estimation/{itemId}")]
        public async Task<IActionResult> GetAllReviewsByItemIdByEstimationAsync([FromRoute] Guid itemId, [FromQuery] OrderByEstimation orderByEstimation,
            [FromQuery] Pagination pagination)
        {
            var reviews =
                await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, orderByEstimation, pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage =
                await unitOfWork.ReviewRepository.GetByItemIdAsync(itemId, orderByEstimation, pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { Reviews = reviews, IsNextPageExisted = reviewsNextPage.Count > 0 });
        }

        [HttpGet]
        [Route("get-by-item-id-by-actuality/{itemId}")]
        public async Task<IActionResult> GetAllReviewsByItemIdByActualityAsync([FromRoute] Guid itemId, [FromQuery] Pagination pagination)
        {
            var reviews =
                await unitOfWork.ReviewRepository.GetByItemIdByActualityAsync(itemId, pagination.PageNumber, pagination.PageSize);

            var reviewsNextPage =
                await unitOfWork.ReviewRepository.GetByItemIdByActualityAsync(itemId, pagination.PageNumber + 1, pagination.PageSize);

            return Ok(new ReviewsResult { IsNextPageExisted = reviewsNextPage.Count > 0, Reviews = reviews });
        }

        [Authorize(Roles = RoleNames.Verified)]
        [HttpGet]
        [Route("react/{reviewId}")]
        public async Task<IActionResult> ReactOnReviewAsync([FromRoute] Guid reviewId, [FromQuery] ReactionType reactionType)
        {
            var userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            try
            {
                await reactionService.ReactAsync(userId, reviewId, reactionType);
            }
            catch (NotFoundException exc)
            {
                return NotFound(exc.Message);
            }
            catch (SelfReactionNotAllowedException)
            {
                return BadRequest("User cannot react on review created by him");
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "An exception was thrown while processing react on review method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateReviewAsync([FromBody] UpdateReviewDto model)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(model.Id);
            if (review == null)
                return NotFound("Review with current identifier does not exist");

            if (review.ReviewStatus != EntityStatus.UnderConsideration)
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

        [Authorize(Roles = RoleNames.Verified)]
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
            EntityStatus reviewStatus = review.ReviewStatus;
            int itemEstimation = review.ItemEstimation;

            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.ReviewRepository.RemoveAsync(reviewId);
                await unitOfWork.CompleteAsync();

                var commentIdsToDelete = await unitOfWork.CommentRepository.GetAllCommentIdsByReviewIdAsync(reviewId);
                await unitOfWork.CommentReplyRepository.ExecuteDeleteAllRelationshipsByIdsAsync(commentIdsToDelete);
                await unitOfWork.CommentRepository.ExecuteDeleteCommentsByReviewIdAsync(reviewId);

                await messagePublisher.PublishAsync(new ReviewRemovedEvent
                    { ItemId = itemId, IsReviewVerified = reviewStatus == EntityStatus.Verified, ItemEstimation = itemEstimation});

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

        [UserRestrictionFilter(RestrictionGrpcService.RestrictionType.All, RestrictionGrpcService.RestrictionType.ReviewPosting)]
        [RequestSizeLimit(5 * 2 * 1024 * 1024)]
        [Authorize(Roles = RoleNames.Verified)]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> SuggestReviewAsync([FromBody] AddReviewDto model)
        {
            if (model == null)
                return BadRequest("Request size exceeds the limit");
            
            if (model.ItemEstimation < 1 || model.ItemEstimation > 5)
                return BadRequest("Incorrect item estimation");

            foreach (var pictureSource in model.Pictures)
            {
                if(!imageValidator.IsImage(pictureSource)) return BadRequest("Incorrect picture format");
            }

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            var reviewToAdd = new Review
            {
                Id = Guid.NewGuid(), UserId = userId, ItemId = model.ItemId, CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                IsCreatedWithItem = false, DislikesCount = 0, LikesCount = 0, ItemEstimation = model.ItemEstimation,
                RejectionReason = null, ReviewStatus = EntityStatus.UnderConsideration, ShortReview = model.ShortReview,
                Text = model.Text, Pictures = model.Pictures, CommentsCount = 0
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

            if (review.ReviewStatus != EntityStatus.UnderConsideration)
                return BadRequest("Review is not in Under consideration status");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                review.ReviewStatus = EntityStatus.Verified;
                review.CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
                unitOfWork.ReviewRepository.Update(review);
                await unitOfWork.CompleteAsync();

                await messagePublisher.PublishAsync(new ReviewAcceptedEvent
                {
                    ItemId = review.ItemId, IsReviewCreatedWithItem = review.IsCreatedWithItem,
                    ItemEstimation = review.ItemEstimation, ReviewId = review.Id
                });

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

            if (review.ReviewStatus != EntityStatus.UnderConsideration)
                return BadRequest("Review is not in Under consideration status");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid currentUserId = new Guid(userIdStr);
            try
            {
                await unitOfWork.BeginTransactionAsync();

                review.ReviewStatus = EntityStatus.Rejected;
                review.RejectionReason = model.RejectionReason;
                unitOfWork.ReviewRepository.Update(review);
                await unitOfWork.CompleteAsync();

                if (model.AddRestriction != null)
                {
                    await messagePublisher.PublishAsync(new ReviewRejectedSagaEvent
                    {
                        ReviewId = review.Id, Duration = model.AddRestriction.Duration,
                        RestrictingUserId = currentUserId,
                        IsPermanent = model.AddRestriction.IsPermanent, Reason = model.AddRestriction.Reason,
                        RestrictedUserId = review.UserId, RestrictionType = (int)model.AddRestriction.RestrictionType,
                    });
                }
                else
                {
                    if (review.IsCreatedWithItem)
                        await messagePublisher.PublishAsync(new ReviewCreatedWithItemRejectedEvent { ItemId = review.ItemId });
                }

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing review rejecting method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            logger.LogInformation("User {UserId} rejected review {ReviewId}", userIdStr, model.ReviewId);

            if (model.AddRestriction != null) return Accepted();

            return Ok();
        }
    }
}