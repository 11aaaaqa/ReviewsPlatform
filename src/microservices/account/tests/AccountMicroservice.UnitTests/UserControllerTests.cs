using AccountMicroservice.Api.Controllers;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.EmailServices;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccountMicroservice.UnitTests
{
    public class UserControllerTests
    {
        [Fact]
        public async Task GetRefreshTokenAsync_ReturnsBadRequest()
        {
            Guid userId = Guid.NewGuid();
            string incorrectSecret = "IncorrectSecret";
            string correctSecret = "CorrectSecret";
            var mock = new Mock<IConfiguration>();
            mock.Setup(x => x["INTERNAL_ENDPOINT_SECRET"]).Returns(correctSecret);
            var controller = new UserController(new Mock<IPasswordService>().Object, new Mock<IUnitOfWork>().Object,
                new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object, new Mock<ILogger<UserController>>().Object,
                mock.Object, new Mock<ITokenService>().Object, new Mock<IEmailService>().Object);

            var result = await controller.GetRefreshToken(userId, incorrectSecret);

            Assert.IsType<BadRequestResult>(result);
            mock.Verify(x => x["INTERNAL_ENDPOINT_SECRET"]);
        }

        [Fact]
        public async Task GetRefreshTokenAsync_ReturnsNotFound()
        {
            Guid userId = Guid.NewGuid();
            string correctSecret = "CorrectSecret";
            var configurationMock = new Mock<IConfiguration>();
            var uowMock = new Mock<IUnitOfWork>();
            configurationMock.Setup(x => x["INTERNAL_ENDPOINT_SECRET"]).Returns(correctSecret);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);
            var controller = new UserController(new Mock<IPasswordService>().Object, uowMock.Object,
                new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object, new Mock<ILogger<UserController>>().Object,
                configurationMock.Object, new Mock<ITokenService>().Object, new Mock<IEmailService>().Object);

            var result = await controller.GetRefreshToken(userId, correctSecret);

            Assert.IsType<NotFoundObjectResult>(result);
            configurationMock.Verify(x => x["INTERNAL_ENDPOINT_SECRET"]);
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
        }
    }
}
