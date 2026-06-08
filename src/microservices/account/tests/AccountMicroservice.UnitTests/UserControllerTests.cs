using System.Security.Claims;
using AccountMicroservice.Api.Controllers;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.EmailServices;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Http;
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

        [Fact]
        public async Task UpdateUserNameAsync_ReturnsConflict()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            string newUserName = "newUserName";
            string oldUserName = "oldUserName";
            UpdateUserNameDto model = new UpdateUserNameDto { NewUserName = newUserName };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(new User { Id = userId, UserName = oldUserName });
            mock.Setup(x => x.UserService.GetUserByUserNameAsync(newUserName))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = newUserName });
            var controller = new UserController(new Mock<IPasswordService>().Object, mock.Object,
                new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object, new Mock<ILogger<UserController>>().Object,
                new Mock<IConfiguration>().Object, new Mock<ITokenService>().Object, new Mock<IEmailService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.UpdateUserNameAsync(model);

            Assert.IsType<ConflictObjectResult>(result);
            mock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            mock.Verify(x => x.UserService.GetUserByUserNameAsync(newUserName));
        }

        [Fact]
        public async Task UpdateUserNameAsync_ReturnsOkAvatarDefault()
        {
            Guid userId = Guid.NewGuid();
            var userContext = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            string newUserName = "newUserName";
            string oldUserName = "oldUserName";
            string returnedAccessToken = "accessToken";
            UpdateUserNameDto model = new UpdateUserNameDto { NewUserName = newUserName };
            User user = new User { Id = userId, UserName = oldUserName, IsAvatarDefault = true};
            var uowMock = new Mock<IUnitOfWork>();
            var avatarMock = new Mock<IAvatarService>();
            var tokenMock = new Mock<ITokenService>();
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            uowMock.Setup(x => x.UserService.GetUserByUserNameAsync(newUserName)).ReturnsAsync((User?)null);
            avatarMock.Setup(x => x.GetDefaultUserAvatar(user, It.IsAny<int>())).Returns(It.IsAny<byte[]>());
            tokenMock.Setup(x => x.GenerateRefreshToken()).Returns(It.IsAny<string>());
            uowMock.Setup(x => x.UserService.UpdateUser(user));
            uowMock.Setup(x => x.CompleteAsync());
            tokenMock.Setup(x => x.GetClaims(user)).Returns(It.IsAny<List<Claim>>());
            tokenMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns(returnedAccessToken);
            var controller = new UserController(new Mock<IPasswordService>().Object, uowMock.Object,
                new Mock<IRoleService>().Object, avatarMock.Object, new Mock<ILogger<UserController>>().Object,
                new Mock<IConfiguration>().Object, tokenMock.Object, new Mock<IEmailService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userContext } };
                    
            var result = await controller.UpdateUserNameAsync(model);

            var methodResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(returnedAccessToken, methodResult.Value);
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.Verify(x => x.UserService.GetUserByUserNameAsync(newUserName));
            avatarMock.Verify(x => x.GetDefaultUserAvatar(user, It.IsAny<int>()));
            tokenMock.Verify(x => x.GenerateRefreshToken());
            uowMock.Verify(x => x.UserService.UpdateUser(user));
            uowMock.Verify(x => x.CompleteAsync());
            tokenMock.Verify(x => x.GetClaims(user));
            tokenMock.Verify(x => x.GenerateAccessToken(It.IsAny<List<Claim>>()));
        }

        [Fact]
        public async Task UpdateUserNameAsync_ReturnsOkEqualUserNames()
        {
            Guid userId = Guid.NewGuid();
            var userContext = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            string newUserName = "newUserName";
            string returnedAccessToken = "accessToken";
            UpdateUserNameDto model = new UpdateUserNameDto { NewUserName = newUserName };
            User user = new User { Id = userId, UserName = newUserName, IsAvatarDefault = false };
            var uowMock = new Mock<IUnitOfWork>();
            var tokenMock = new Mock<ITokenService>();
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            uowMock.Setup(x => x.UserService.GetUserByUserNameAsync(newUserName))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = newUserName });
            tokenMock.Setup(x => x.GenerateRefreshToken()).Returns(It.IsAny<string>());
            uowMock.Setup(x => x.UserService.UpdateUser(user));
            uowMock.Setup(x => x.CompleteAsync());
            tokenMock.Setup(x => x.GetClaims(user)).Returns(It.IsAny<List<Claim>>());
            tokenMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns(returnedAccessToken);
            var controller = new UserController(new Mock<IPasswordService>().Object, uowMock.Object,
                new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object, new Mock<ILogger<UserController>>().Object,
                new Mock<IConfiguration>().Object, tokenMock.Object, new Mock<IEmailService>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userContext } };

            var result = await controller.UpdateUserNameAsync(model);

            var methodResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(returnedAccessToken, methodResult.Value);
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.Verify(x => x.UserService.GetUserByUserNameAsync(newUserName));
            tokenMock.Verify(x => x.GenerateRefreshToken());
            uowMock.Verify(x => x.UserService.UpdateUser(user));
            uowMock.Verify(x => x.CompleteAsync());
            tokenMock.Verify(x => x.GetClaims(user));
            tokenMock.Verify(x => x.GenerateAccessToken(It.IsAny<List<Claim>>()));
        }
    }
}
