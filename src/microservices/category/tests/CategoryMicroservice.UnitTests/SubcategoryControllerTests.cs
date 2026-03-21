using System.Security.Claims;
using CategoryMicroservice.Api.Controllers;
using CategoryMicroservice.Api.DTOs.Subcategory;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Abstractions;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CategoryMicroservice.UnitTests
{
    public class SubcategoryControllerTests
    {
        [Fact]
        public async Task AddSubcategoryAsync_ReturnsConflict()
        {
            Guid categoryId = Guid.NewGuid();
            string subcategoryName = "SubcategoryName";
            var model = new AddSubcategoryDto { CategoryId = categoryId, Name = subcategoryName };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.SubcategoryRepository.FindByNameAsync(model.Name))
                .ReturnsAsync(new Subcategory { Name = subcategoryName });
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, new Mock<IMessagePublisher>().Object);

            var result = await controller.AddSubcategoryAsync(model);

            Assert.IsType<ConflictObjectResult>(result);
            uowMock.Verify(x => x.SubcategoryRepository.FindByNameAsync(model.Name));
        }

        [Fact]
        public async Task AddSubcategoryAsync_ReturnsBadRequest()
        {
            Guid categoryId = Guid.NewGuid();
            string subcategoryName = "SubcategoryName";
            var model = new AddSubcategoryDto { CategoryId = categoryId, Name = subcategoryName };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.SubcategoryRepository.FindByNameAsync(model.Name)).ReturnsAsync((Subcategory?)null);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(model.CategoryId)).ReturnsAsync((Category?)null);
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, new Mock<IMessagePublisher>().Object);

            var result = await controller.AddSubcategoryAsync(model);

            Assert.IsType<BadRequestObjectResult>(result);
            uowMock.Verify(x => x.SubcategoryRepository.FindByNameAsync(model.Name));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(model.CategoryId));
        }

        [Fact]
        public async Task AddSubcategoryAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid categoryId = Guid.NewGuid();
            string subcategoryName = "SubcategoryName";
            var model = new AddSubcategoryDto { CategoryId = categoryId, Name = subcategoryName };
            var category = new Category { Id = categoryId };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.SubcategoryRepository.FindByNameAsync(model.Name)).ReturnsAsync((Subcategory?)null);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(model.CategoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.SubcategoryRepository.AddAsync(It.IsAny<Subcategory>()));
            uowMock.Setup(x => x.CompleteAsync());
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, new Mock<IMessagePublisher>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.AddSubcategoryAsync(model);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.SubcategoryRepository.FindByNameAsync(model.Name));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(model.CategoryId));
            uowMock.Verify(x => x.SubcategoryRepository.AddAsync(It.IsAny<Subcategory>()));
            uowMock.Verify(x => x.CompleteAsync());
        }

        [Fact]
        public async Task RemoveSubcategoryAsync_ReturnsNotFound()
        {
            Guid subcategoryId = Guid.NewGuid();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId)).ReturnsAsync((Subcategory?)null);
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, new Mock<IMessagePublisher>().Object);

            var result = await controller.RemoveSubcategoryAsync(subcategoryId);

            Assert.IsType<NotFoundObjectResult>(result);
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId));
        }

        [Fact]
        public async Task RemoveSubcategoryAsync_ReturnsInternalServerErrorOnCompleting()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid subcategoryId = Guid.NewGuid();
            Guid categoryId = Guid.NewGuid();
            int subcategoryReviewsCount = 5;
            var subcategory = new Subcategory { Id = subcategoryId, CategoryId = categoryId, ReviewsCount = subcategoryReviewsCount};
            int categoryReviewsCount = 10;
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Setup(x => x.CompleteAsync()).Throws<Exception>();
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, new Mock<IMessagePublisher>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveSubcategoryAsync(subcategoryId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Verify(x => x.CompleteAsync());
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveSubcategoryAsync_ReturnsInternalServerErrorOnPublishing()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid subcategoryId = Guid.NewGuid();
            Guid categoryId = Guid.NewGuid();
            int subcategoryReviewsCount = 5;
            var subcategory = new Subcategory { Id = subcategoryId, CategoryId = categoryId, ReviewsCount = subcategoryReviewsCount };
            int categoryReviewsCount = 10;
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>())).Throws<Exception>();
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, messagePublisherMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveSubcategoryAsync(subcategoryId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveSubcategoryAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid subcategoryId = Guid.NewGuid();
            Guid categoryId = Guid.NewGuid();
            int subcategoryReviewsCount = 5;
            var subcategory = new Subcategory { Id = subcategoryId, CategoryId = categoryId, ReviewsCount = subcategoryReviewsCount };
            int categoryReviewsCount = 10;
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(categoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Setup(x => x.CommitTransactionAsync());
            var controller = new SubcategoryController(uowMock.Object,
                new Mock<ILogger<SubcategoryController>>().Object, messagePublisherMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveSubcategoryAsync(subcategoryId);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(subcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(categoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.SubcategoryRepository.RemoveAsync(subcategoryId));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }
    }
}
