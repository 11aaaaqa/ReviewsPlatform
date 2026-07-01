using System.Security.Claims;
using MessageBus.Messages.Comment;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestrictionGrpcService;
using ReviewMicroservice.Api.Constants;
using ReviewMicroservice.Api.DTOs;
using ReviewMicroservice.Api.DTOs.comment;
using ReviewMicroservice.Api.Models;
using ReviewMicroservice.Api.Models.Business.Comments;
using ReviewMicroservice.Api.Services.UnitOfWork;

namespace ReviewMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController(IUnitOfWork unitOfWork, RestrictionInfo.RestrictionInfoClient restrictionInfoClient,
        ILogger<CommentController> logger, IMessagePublisher messagePublisher) : ControllerBase
    {
        [HttpGet]
        [Route("get-by-id/{commentId}")]
        public async Task<IActionResult> GetCommentByIdAsync([FromRoute] Guid commentId)
        {
            var comment = await unitOfWork.CommentRepository.GetByIdAsync(commentId);
            if (comment == null) 
                return NotFound();

            return Ok(comment);
        }

        [HttpGet]
        [Route("get-by-review-id/{reviewId}")]
        public async Task<IActionResult> GetCommentsByReviewIdAsync([FromRoute] Guid reviewId, [FromQuery] Pagination pagination)
        {
            var comments = await unitOfWork.CommentRepository.GetByReviewIdAsync(
                reviewId, pagination.PageSize, pagination.PageNumber);

            var commentsNextPage = await unitOfWork.CommentRepository.GetByReviewIdAsync(
                reviewId, pagination.PageSize, pagination.PageNumber + 1);

            return Ok(new CommentsResult { Comments = comments, IsNextPageExisted = commentsNextPage.Count > 0 });
        }

        [HttpGet]
        [Route("get-replies/{parentCommentId}")]
        public async Task<IActionResult> GetCommentRepliesAsync([FromRoute] Guid parentCommentId, [FromQuery] Pagination pagination)
        {
            var comments = await unitOfWork.CommentRepository
                .GetCommentRepliesAsync(parentCommentId, pagination.PageSize, pagination.PageNumber);

            var commentsNextPage = await unitOfWork.CommentRepository
                .GetCommentRepliesAsync(parentCommentId, pagination.PageSize, pagination.PageNumber + 1);

            return Ok(new CommentsResult { Comments = comments, IsNextPageExisted = commentsNextPage.Count > 0 });
        }

        [HttpGet]
        [Route("get-by-user-id/{userId}")]
        public async Task<IActionResult> GetCommentsByUserIdAsync([FromRoute] Guid userId, [FromQuery] Pagination pagination)
        {
            var comments = await unitOfWork.CommentRepository.GetByUserIdAsync(userId, pagination.PageSize, pagination.PageNumber);

            var commentsNextPage = await unitOfWork.CommentRepository
                .GetByUserIdAsync(userId, pagination.PageSize, pagination.PageNumber + 1);

            return Ok(new CommentsResult { Comments = comments, IsNextPageExisted = commentsNextPage.Count > 0 });
        }

        [Authorize(Roles = RoleNames.Verified)]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddCommentAsync([FromBody] AddCommentDto model)
        {
            var review = await unitOfWork.ReviewRepository.GetByIdAsync(model.ReviewId);
            if (review == null) return NotFound("Review with current identifier does not exist");

            if (model.ParentCommentId != null)
            {
                var parentComment = await unitOfWork.CommentRepository.GetByIdAsync(model.ParentCommentId.Value);
                if (parentComment == null) return NotFound("Comment you are trying to reply to does not exist");
            }

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            try
            {
                var restrictionInfoReply = await restrictionInfoClient.GetRestrictionInfoAsync(
                    new GetRestrictionInfoRequest { UserId = userIdStr });
                if (restrictionInfoReply.RestrictionType == RestrictionType.All || restrictionInfoReply.RestrictionType == RestrictionType.Commenting)
                    return Forbid();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Rpc call threw an exception while trying to reach Restriction microservice");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            await unitOfWork.CommentRepository.AddAsync(new Comment
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CommentStatus = CommentStatus.UnderConsideration,
                UserId = userId,
                RepliesCount = 0,
                ConsideredByUserId = null,
                RejectionReason = null,
                ParentCommentId = model.ParentCommentId,
                ReviewId = model.ReviewId,
                Text = model.Text,
            });
            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete]
        [Route("remove/{commentId}")]
        public async Task<IActionResult> RemoveCommentAsync([FromRoute] Guid commentId)
        {
            var comment = await unitOfWork.CommentRepository.GetByIdAsync(commentId);
            if (comment == null) return NotFound("Comment with current identifier does not exist");

            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid currentUserId = new Guid(currentUserIdStr);
            if(comment.UserId != currentUserId && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Moderator))
                return Forbid();

            try
            {
                await unitOfWork.BeginTransactionAsync();

                int commentsCountToDecrease = -(comment.RepliesCount + 1);
                if (comment.ParentCommentId != null)
                {
                    List<Guid> commentIdsToUpdateRepliesCount = await unitOfWork.CommentReplyRepository.GetCommentAncestorIdsAsync(comment.Id);
                    await unitOfWork.CommentRepository.ExecuteRepliesCountUpdateAsync(commentIdsToUpdateRepliesCount, commentsCountToDecrease);
                }
                List<Guid> commentIdsToDelete = await unitOfWork.CommentReplyRepository.GetCommentDescendantIdsAsync(comment.Id);
                commentIdsToDelete.Add(comment.Id);
                await unitOfWork.CommentReplyRepository.ExecuteDeleteAllRelationshipsByIdsAsync(commentIdsToDelete);

                await unitOfWork.CommentRepository.ExecuteDeleteCommentsByIdsAsync(commentIdsToDelete);
                await unitOfWork.CommentRepository.ExecuteDeleteCommentsByParentIdsAsync(commentIdsToDelete);

                await unitOfWork.ReviewRepository.ExecuteCommentsCountUpdateAsync(comment.ReviewId, commentsCountToDecrease);

                await messagePublisher.PublishAsync(new CommentRemovedEvent { CommentIds = commentIdsToDelete });

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "An exception was thrown while processing comment removing method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpGet]
        [Route("accept-comment/{commentId}")]
        public async Task<IActionResult> AcceptCommentAsync([FromRoute] Guid commentId)
        {
            var comment = await unitOfWork.CommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return NotFound("Comment with current identifier does not exist");

            if (comment.CommentStatus != CommentStatus.UnderConsideration)
                return BadRequest("Comment is not in Under consideration status");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            try
            {
                await unitOfWork.BeginTransactionAsync();

                comment.CommentStatus = CommentStatus.Verified;
                comment.ConsideredByUserId = userId;
                unitOfWork.CommentRepository.Update(comment);
                await unitOfWork.CompleteAsync();

                if (comment.ParentCommentId != null)
                {
                    Guid parentCommentId = comment.ParentCommentId.Value;
                    List<Guid> commentAncestorIds = await unitOfWork.CommentReplyRepository.GetCommentAncestorIdsAsync(parentCommentId);
                    commentAncestorIds.Add(parentCommentId);
                    List<CommentReply> commentRepliesToAdd = new();
                    foreach (Guid ancestorId in commentAncestorIds)
                    {
                        commentRepliesToAdd.Add(new CommentReply { ParentId = ancestorId, RepliedId = comment.Id });
                    }
                    await unitOfWork.CommentReplyRepository.AddRangeAsync(commentRepliesToAdd);
                    await unitOfWork.CompleteAsync();

                    await unitOfWork.CommentRepository.ExecuteRepliesCountUpdateAsync(commentAncestorIds, 1);
                }

                await unitOfWork.ReviewRepository.ExecuteCommentsCountUpdateAsync(comment.ReviewId, 1);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(e, "Exception was thrown while processing accept comment method");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
        [HttpPut]
        [Route("reject-comment")]
        public async Task<IActionResult> RejectCommentAsync([FromBody] RejectCommentDto model)
        {
            var comment = await unitOfWork.CommentRepository.GetByIdAsync(model.CommentId);
            if (comment == null)
                return NotFound("Comment with current identifier does not exist");

            if (comment.CommentStatus != CommentStatus.UnderConsideration)
                return BadRequest("Comment is not in Under consideration status");

            model.Reason = model.Reason.Trim();

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            comment.CommentStatus = CommentStatus.Rejected;
            comment.RejectionReason = model.Reason;
            comment.ConsideredByUserId = userId;
            unitOfWork.CommentRepository.Update(comment);

            await unitOfWork.CompleteAsync();

            return Ok();
        }
    }
 }
