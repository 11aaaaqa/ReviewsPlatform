using System.Security.Claims;
using CategoryMicroservice.Api.Controllers;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Abstractions;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CategoryMicroservice.UnitTests
{
    public class CategoryControllerTests
    {
        [Fact]
        public async Task RemoveCategoryAsync_ReturnsNotFound()
        {
            Guid categoryId = Guid.NewGuid();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);
            var controller = new CategoryController(new Mock<ICategoryRepository<Category>>().Object, uowMock.Object,
                new Mock<ILogger<CategoryController>>().Object, new Mock<IMessagePublisher>().Object);

            var result = await controller.RemoveCategoryAsync(categoryId);

            Assert.IsType<NotFoundObjectResult>(result);
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
        }

        [Fact]
        public async Task RemoveCategoryAsync_ReturnsInternalServerErrorOnCompleting()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid categoryId = Guid.NewGuid();
            Category category = new Category { Id = categoryId };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.RemoveAsync(categoryId));
            uowMock.Setup(x => x.CompleteAsync()).Throws<Exception>();
            var controller = new CategoryController(new Mock<ICategoryRepository<Category>>().Object, uowMock.Object,
                new Mock<ILogger<CategoryController>>().Object, new Mock<IMessagePublisher>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveCategoryAsync(categoryId);

             var methodResult = Assert.IsType<StatusCodeResult>(result);
             Assert.Equal(500, methodResult.StatusCode);
             uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
             uowMock.Verify(x => x.BeginTransactionAsync());
             uowMock.Verify(x => x.CategoryRepository.RemoveAsync(categoryId));
             uowMock.Verify(x => x.CompleteAsync());
             uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveCategoryAsync_ReturnsInternalServerErrorOnPublishing()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid categoryId = Guid.NewGuid();
            Category category = new Category { Id = categoryId };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.RemoveAsync(categoryId));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>())).Throws<Exception>();
            var controller = new CategoryController(new Mock<ICategoryRepository<Category>>().Object, uowMock.Object,
                new Mock<ILogger<CategoryController>>().Object, messagePublisherMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveCategoryAsync(categoryId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, methodResult.StatusCode);
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.CategoryRepository.RemoveAsync(categoryId));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveCategoryAsync_ReturnsOk()
        {
            Guid categoryId = Guid.NewGuid();
            Category category = new Category { Id = categoryId };
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.RemoveAsync(categoryId));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Setup(x => x.CommitTransactionAsync());
            var controller = new CategoryController(new Mock<ICategoryRepository<Category>>().Object, uowMock.Object,
                new Mock<ILogger<CategoryController>>().Object, messagePublisherMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveCategoryAsync(categoryId);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.CategoryRepository.RemoveAsync(categoryId));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }
    }
}
