using System.Security.Claims;
using CategoryMicroservice.Api.Controllers;
using CategoryMicroservice.Api.DTOs.Items;
using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services;
using CategoryMicroservice.Api.Services.UnitOfWork;
using Grpc.Core;
using MessageBus.Abstractions;
using MessageBus.Publisher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RestrictionGrpcService;

namespace CategoryMicroservice.UnitTests
{
    public class ItemControllerTests
    {
        [Fact]
        public async Task UpdateItemAsync_ReturnsNotFound()
        {
            Guid itemId = Guid.NewGuid();
            var model = new UpdateItemDto { Id = itemId, Brand = "NewBrand", Name = "NewName" };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);
            var controller = new ItemController(uowMock.Object, new Mock<IMessagePublisher>().Object, 
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);

            var result = await controller.UpdateItemAsync(model);

            Assert.IsType<NotFoundObjectResult>(result);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnsBadRequest()
        {
            Guid itemId = Guid.NewGuid();
            ItemStatus itemStatus = ItemStatus.Verified;
            var item = new Item { Id = itemId, Status = itemStatus };
            var model = new UpdateItemDto { Id = itemId, Brand = "NewBrand", Name = "NewName" };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync(item);
            var controller = new ItemController(uowMock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);

            var result = await controller.UpdateItemAsync(model);

            Assert.IsType<BadRequestObjectResult>(result);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid itemId = Guid.NewGuid();
            ItemStatus itemStatus = ItemStatus.UnderConsideration;
            var item = new Item { Id = itemId, Status = itemStatus, Brand = "Brand", Name = "Name" };
            var model = new UpdateItemDto { Id = itemId, Brand = "NewBrand", Name = "NewName" };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync(item);
            uowMock.Setup(x => x.ItemRepository.Update(item));
            uowMock.Setup(x => x.CompleteAsync());
            var controller = new ItemController(uowMock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.UpdateItemAsync(model);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
            uowMock.Verify(x => x.ItemRepository.Update(item));
            uowMock.Verify(x => x.CompleteAsync());
        }

        [Fact]
        public async Task RemoveItemAsync_ReturnsNotFound()
        {
            Guid itemId = Guid.NewGuid();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);
            var controller = new ItemController(uowMock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);

            var result = await controller.RemoveItemAsync(itemId);

            Assert.IsType<NotFoundObjectResult>(result);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
        }

        [Fact]
        public async Task RemoveItemAsync_ReturnsInternalServerErrorOnCompleting()
        {
            Guid itemId = Guid.NewGuid();
            int itemReviewsCount = 5;
            Guid subcategoryId = Guid.NewGuid();
            int subcategoryReviewsCount = itemReviewsCount + 3;
            Guid categoryId = Guid.NewGuid();
            int categoryReviewsCount = subcategoryReviewsCount + 4;
            var subcategory = new Subcategory
                { Id = subcategoryId, ReviewsCount = subcategoryReviewsCount, CategoryId = categoryId };
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var item = new Item { Id = itemId, SubcategoryId = subcategoryId, ReviewsCount = itemReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync(item);
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Setup(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.CompleteAsync()).Throws<Exception>();
            var controller = new ItemController(uowMock.Object, new Mock<IMessagePublisher>().Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);

            var result = await controller.RemoveItemAsync(itemId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.CompleteAsync());
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveItemAsync_ReturnsInternalServerErrorPublishing()
        {
            Guid itemId = Guid.NewGuid();
            int itemReviewsCount = 5;
            Guid subcategoryId = Guid.NewGuid();
            int subcategoryReviewsCount = itemReviewsCount + 3;
            Guid categoryId = Guid.NewGuid();
            int categoryReviewsCount = subcategoryReviewsCount + 4;
            var subcategory = new Subcategory
            { Id = subcategoryId, ReviewsCount = subcategoryReviewsCount, CategoryId = categoryId };
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var item = new Item { Id = itemId, SubcategoryId = subcategoryId, ReviewsCount = itemReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync(item);
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Setup(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>())).Throws<Exception>();
            var controller = new ItemController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);

            var result = await controller.RemoveItemAsync(itemId);

            var methodResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, methodResult.StatusCode);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.RollbackTransactionAsync());
        }

        [Fact]
        public async Task RemoveItemAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            Guid itemId = Guid.NewGuid();
            int itemReviewsCount = 5;
            Guid subcategoryId = Guid.NewGuid();
            int subcategoryReviewsCount = itemReviewsCount + 3;
            Guid categoryId = Guid.NewGuid();
            int categoryReviewsCount = subcategoryReviewsCount + 4;
            var subcategory = new Subcategory
            { Id = subcategoryId, ReviewsCount = subcategoryReviewsCount, CategoryId = categoryId };
            var category = new Category { Id = categoryId, ReviewsCount = categoryReviewsCount };
            var item = new Item { Id = itemId, SubcategoryId = subcategoryId, ReviewsCount = itemReviewsCount };
            var uowMock = new Mock<IUnitOfWork>();
            var messagePublisherMock = new Mock<IMessagePublisher>();
            uowMock.Setup(x => x.ItemRepository.GetByIdAsync(itemId)).ReturnsAsync(item);
            uowMock.Setup(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId)).ReturnsAsync(subcategory);
            uowMock.Setup(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId)).ReturnsAsync(category);
            uowMock.Setup(x => x.BeginTransactionAsync());
            uowMock.Setup(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Setup(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Setup(x => x.CategoryRepository.Update(category));
            uowMock.Setup(x => x.CompleteAsync());
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Setup(x => x.CommitTransactionAsync());
            var controller = new ItemController(uowMock.Object, messagePublisherMock.Object,
                new Mock<ILogger<ItemController>>().Object, new Mock<ImageValidator>().Object,
                new Mock<RestrictionInfo.RestrictionInfoClient>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RemoveItemAsync(itemId);

            Assert.IsType<OkResult>(result);
            uowMock.Verify(x => x.ItemRepository.GetByIdAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.GetByIdAsync(item.SubcategoryId));
            uowMock.Verify(x => x.CategoryRepository.GetByIdAsync(subcategory.CategoryId));
            uowMock.Verify(x => x.BeginTransactionAsync());
            uowMock.Verify(x => x.ItemRepository.RemoveAsync(itemId));
            uowMock.Verify(x => x.SubcategoryRepository.Update(subcategory));
            uowMock.Verify(x => x.CategoryRepository.Update(category));
            uowMock.Verify(x => x.CompleteAsync());
            messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<MessageBase>()));
            uowMock.Verify(x => x.CommitTransactionAsync());
        }
    }
}