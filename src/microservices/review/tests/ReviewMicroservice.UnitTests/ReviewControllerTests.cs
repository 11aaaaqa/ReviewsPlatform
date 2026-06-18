using System.Security.Claims;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RestrictionGrpcService;
using ReviewMicroservice.Api.Controllers;
using ReviewMicroservice.Api.Enums;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Services;
using ReviewMicroservice.Api.Services.ReviewServices.ReactionServices;
using ReviewMicroservice.Api.Services.UnitOfWork;

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
                new Mock<ILogger<ReviewController>>().Object, new Mock<RestrictionInfo.RestrictionInfoClient>().Object,
                new Mock<ImageValidator>().Object, reactionMock.Object);
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
                new Mock<ILogger<ReviewController>>().Object, new Mock<RestrictionInfo.RestrictionInfoClient>().Object,
                new Mock<ImageValidator>().Object, reactionMock.Object);
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
                new Mock<ILogger<ReviewController>>().Object, new Mock<RestrictionInfo.RestrictionInfoClient>().Object,
                new Mock<ImageValidator>().Object, reactionMock.Object);
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
                new Mock<ILogger<ReviewController>>().Object, new Mock<RestrictionInfo.RestrictionInfoClient>().Object,
                new Mock<ImageValidator>().Object, reactionMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.ReactOnReviewAsync(reviewId, reactionType);

            Assert.IsType<OkResult>(result);
            reactionMock.Verify(x => x.ReactAsync(userId, reviewId, reactionType));
        }
    }
}