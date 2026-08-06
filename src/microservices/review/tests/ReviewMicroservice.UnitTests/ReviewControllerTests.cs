using MessageBus.Messages.Review;
using MessageBus.Messages.Saga.RejectReviewAndAddRestriction;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewMicroservice.Api.Controllers;
using ReviewMicroservice.Api.DTOs.review;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Models.Business;
using ReviewMicroservice.Api.Services;
using ReviewMicroservice.Api.Services.ReviewServices.ReactionServices;
using ReviewMicroservice.Api.Services.UnitOfWork;
using System.Security.Claims;

namespace ReviewMicroservice.UnitTests
{
    public class ReviewControllerTests
    {
        [Fact]
        public async Task ReactOnReviewAsync_ReturnsNotFound()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var reviewId = Guid.NewGuid();
            var reactionType = ReactionType.Like;
            var reactionMock = new Mock<IReactionService>();
            reactionMock.Setup(x => x.ReactAsync(userId, reviewId, reactionType)).Throws<NotFoundException>();
            var controller = new ReviewController(new Mock<IUnitOfWork>().Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object, reactionMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.ReactOnReviewAsync(reviewId, reactionType);

            Assert.IsType<NotFoundObjectResult>(result);
            reactionMock.Verify(x => x.ReactAsync(userId, reviewId, reactionType));
        }

        [Fact]
        public async Task ReactOnReviewAsync_ReturnsBadRequest()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var reviewId = Guid.NewGuid();
            var reactionType = ReactionType.Like;
            var reactionMock = new Mock<IReactionService>();
            reactionMock.Setup(x => x.ReactAsync(userId, reviewId, reactionType)).Throws<SelfReactionNotAllowedException>();
            var controller = new ReviewController(new Mock<IUnitOfWork>().Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object, reactionMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.ReactOnReviewAsync(reviewId, reactionType);

            Assert.IsType<BadRequestObjectResult>(result);
            reactionMock.Verify(x => x.ReactAsync(userId, reviewId, reactionType));
        }

        [Fact]
        public async Task ReactOnReviewAsync_ReturnsInternalServerError()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var reviewId = Guid.NewGuid();
            var reactionType = ReactionType.Like;
            var reactionMock = new Mock<IReactionService>();
            reactionMock.Setup(x => x.ReactAsync(userId, reviewId, reactionType)).Throws<Exception>();
            var controller = new ReviewController(new Mock<IUnitOfWork>().Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object, reactionMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.ReactOnReviewAsync(reviewId, reactionType);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            reactionMock.Verify(x => x.ReactAsync(userId, reviewId, reactionType));
        }

        [Fact]
        public async Task ReactOnReviewAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var reviewId = Guid.NewGuid();
            var reactionType = ReactionType.Like;
            var reactionMock = new Mock<IReactionService>();
            reactionMock.Setup(x => x.ReactAsync(userId, reviewId, reactionType));
            var controller = new ReviewController(new Mock<IUnitOfWork>().Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object, reactionMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.ReactOnReviewAsync(reviewId, reactionType);

            Assert.IsType<OkResult>(result);
            reactionMock.Verify(x => x.ReactAsync(userId, reviewId, reactionType));
        }

        [Fact]
        public async Task AcceptReviewAsync_ReturnsNotFound()
        {
            Guid reviewId = Guid.NewGuid();
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.AcceptReviewAsync(reviewId);

            Assert.IsType<NotFoundObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(reviewId));
        }

        [Fact]
        public async Task AcceptReviewAsync_ReturnsBadRequestVerifiedStatus()
        {
            Guid reviewId = Guid.NewGuid();
            Review review = new() { Id = reviewId, ReviewStatus = EntityStatus.Verified };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.AcceptReviewAsync(reviewId);

            Assert.IsType<BadRequestObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(reviewId));
        }

        [Fact]
        public async Task AcceptReviewAsync_ReturnsBadRequestRejectedStatus()
        {
            Guid reviewId = Guid.NewGuid();
            Review review = new() { Id = reviewId, ReviewStatus = EntityStatus.Rejected };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.AcceptReviewAsync(reviewId);

            Assert.IsType<BadRequestObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(reviewId));
        }

        [Fact]
        public async Task AcceptReviewAsync_ReturnsInternalServerError()
        {
            Guid reviewId = Guid.NewGuid();
            Review review = new()
            {
                Id = reviewId, ReviewStatus = EntityStatus.UnderConsideration, ItemId = Guid.NewGuid(),
                IsCreatedWithItem = true, ItemEstimation = 3
            };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ReviewRepository.GetByIdAsync(reviewId)).ReturnsAsync(review);
            uowMock.Setup(x => x.ReviewRepository.Update(review));
            uowMock.Setup(x => x.CompleteAsync()).Throws<Exception>();    
            var controller = new ReviewController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.AcceptReviewAsync(reviewId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ReviewRepository.GetByIdAsync(reviewId));
            uowMock.Verify(x => x.ReviewRepository.Update(review));
            uowMock.Verify(x => x.CompleteAsync());
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task AcceptReviewAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid reviewId = Guid.NewGuid();
            Review review = new()
            {
                Id = reviewId,
                ReviewStatus = EntityStatus.UnderConsideration,
                ItemId = Guid.NewGuid(),
                IsCreatedWithItem = true,
                ItemEstimation = 3
            };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ReviewRepository.GetByIdAsync(reviewId)).ReturnsAsync(review);
            uowMock.Setup(x => x.ReviewRepository.Update(review));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<ReviewAcceptedEvent>()));
            var controller = new ReviewController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);
            controller.ControllerContext = new ControllerContext
                { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.AcceptReviewAsync(reviewId);

            Assert.IsType<OkResult>(result);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ReviewRepository.GetByIdAsync(reviewId));
            uowMock.Verify(x => x.ReviewRepository.Update(review));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<ReviewAcceptedEvent>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsNotFound()
        {
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync((Review?)null);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<NotFoundObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsBadRequestVerifiedStatus()
        {
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId };
            var review = new Review { Id = model.ReviewId, ReviewStatus = EntityStatus.Verified };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<BadRequestObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsBadRequestRejectedStatus()
        {
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId };
            var review = new Review { Id = model.ReviewId, ReviewStatus = EntityStatus.Rejected };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<BadRequestObjectResult>(result);
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsInternalServerError()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId };
            var review = new Review { Id = model.ReviewId, ReviewStatus = EntityStatus.UnderConsideration, RejectionReason = null };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            mock.Setup(x => x.ReviewRepository.Update(review));
            mock.Setup(x => x.CompleteAsync()).Throws<Exception>();
            var controller = new ReviewController(mock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RejectReviewAsync(model);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            mock.Verify(x => x.BeginTransactionAsync());
            mock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
            mock.Verify(x => x.ReviewRepository.Update(review));
            mock.Verify(x => x.CompleteAsync());
            mock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsOkWithoutRestrictionCreatedWithItem()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId, AddRestriction = null };
            var review = new Review
            {
                Id = model.ReviewId, ReviewStatus = EntityStatus.UnderConsideration, RejectionReason = null,
                IsCreatedWithItem = true
            };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            uowMock.Setup(x => x.ReviewRepository.Update(review));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<ReviewCreatedWithItemRejectedEvent>()));
            var controller = new ReviewController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
            uowMock.Verify(x => x.ReviewRepository.Update(review));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<ReviewCreatedWithItemRejectedEvent>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsOkWithoutRestrictionCreatedWithoutItem()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId, AddRestriction = null };
            var review = new Review
            {
                Id = model.ReviewId,
                ReviewStatus = EntityStatus.UnderConsideration,
                RejectionReason = null,
                IsCreatedWithItem = false
            };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            uowMock.Setup(x => x.ReviewRepository.Update(review));
            uowMock.Setup(x => x.CompleteAsync());
            var controller = new ReviewController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
            uowMock.Verify(x => x.ReviewRepository.Update(review));
            uowMock.Verify(x => x.CompleteAsync());
            uowMock.Verify(x => x.CommitTransactionAsync());
        }

        [Fact]
        public async Task RejectReviewAsync_ReturnsAcceptedWithRestriction()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid reviewId = Guid.NewGuid();
            string rejectionReason = "reason";
            var restriction = new AddRestrictionDto
            {
                Duration = TimeSpan.Zero, IsPermanent = true, Reason = "reason", RestrictionType = RestrictionType.All
            };
            var model = new RejectReviewDto { RejectionReason = rejectionReason, ReviewId = reviewId, AddRestriction =  restriction };
            var review = new Review { Id = model.ReviewId, ReviewStatus = EntityStatus.UnderConsideration, RejectionReason = null };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ReviewRepository.GetByIdAsync(model.ReviewId)).ReturnsAsync(review);
            uowMock.Setup(x => x.ReviewRepository.Update(review));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<ReviewRejectedSagaEvent>()));
            var controller = new ReviewController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ReviewController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<IReactionService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RejectReviewAsync(model);

            Assert.IsType<AcceptedResult>(result);
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ReviewRepository.GetByIdAsync(model.ReviewId));
            uowMock.Verify(x => x.ReviewRepository.Update(review));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<ReviewRejectedSagaEvent>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }
    }
}