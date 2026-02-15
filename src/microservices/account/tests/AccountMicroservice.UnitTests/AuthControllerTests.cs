using System.Runtime.CompilerServices;
using AccountMicroservice.Api.Controllers;
using AccountMicroservice.Api.DTOs.Auth;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.Models.General;

namespace AccountMicroservice.UnitTests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsConflictName()
        {
            var model = new RegisterDto
                { Email = It.IsAny<string>(), Password = It.IsAny<string>(), UserName = It.IsAny<string>() };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.UserService.GetUserByUserNameAsync(model.UserName))
                .ReturnsAsync(new User { UserName = model.UserName });
            var controller = new AuthController(new Mock<IPasswordService>().Object, new Mock<ITokenService>().Object,
                mock.Object, new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object,
                new Mock<ILogger<AuthController>>().Object);

            var result = await controller.RegisterAsync(model);

            Assert.IsType<ConflictObjectResult>(result);
            mock.Verify(x => x.UserService.GetUserByUserNameAsync(model.UserName));
        }

        [Fact]
        public async Task RegisterAsync_ReturnsConflictEmail()
        {
            var model = new RegisterDto
                { Email = It.IsAny<string>(), Password = It.IsAny<string>(), UserName = It.IsAny<string>() };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(x => x.UserService.GetUserByUserNameAsync(model.UserName)).ReturnsAsync((User?)null);
            mock.Setup(x => x.UserService.GetUserByEmailAsync(model.Email)).ReturnsAsync(new User { Email = model.Email });
            var controller = new AuthController(new Mock<IPasswordService>().Object, new Mock<ITokenService>().Object,
                mock.Object, new Mock<IRoleService>().Object, new Mock<IAvatarService>().Object,
                new Mock<ILogger<AuthController>>().Object);

            var result = await controller.RegisterAsync(model);

            Assert.IsType<ConflictObjectResult>(result);
            mock.Verify(x => x.UserService.GetUserByUserNameAsync(model.UserName));
            mock.Verify(x => x.UserService.GetUserByEmailAsync(model.Email));
        }

        [Fact]
        public async Task RegisterAsync_ReturnsInternalServerError()
        {
            var model = new RegisterDto
                { Email = "Email", Password = "Password", UserName = "userName" };
            var hashPasswordModel = new FormatHashResult { PasswordHash = new byte[3], Salt = new byte[3] };
            byte[] avatarModel = new byte[3];
            var userRoleModel = new Role { Id = Guid.NewGuid(), Name = RoleNames.User };
            var passwordMock = new Mock<IPasswordService>();
            var tokenMock = new Mock<ITokenService>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var roleMock = new Mock<IRoleService>();
            var avatarMock = new Mock<IAvatarService>();

            unitOfWorkMock.Setup(x => x.UserService.GetUserByUserNameAsync(model.UserName)).ReturnsAsync((User?)null);
            unitOfWorkMock.Setup(x => x.UserService.GetUserByEmailAsync(model.Email)).ReturnsAsync((User?)null);
            passwordMock.Setup(x => x.HashPassword(model.Password)).Returns(hashPasswordModel);
            avatarMock.Setup(x => x.GetDefaultUserAvatar(It.IsAny<User>(), 200)).Returns(avatarModel);
            unitOfWorkMock.Setup(x => x.BeginTransactionAsync());
            unitOfWorkMock.Setup(x => x.UserService.AddUserAsync(It.IsAny<User>()));
            roleMock.Setup(x => x.GetRoleByNameAsync(RoleNames.User)).ReturnsAsync(userRoleModel);
            unitOfWorkMock.Setup(x => x.UserRolesService.AddUserToRoleAsync(It.IsAny<Guid>(), It.IsAny<Guid>()));
            unitOfWorkMock.Setup(x => x.CommitTransactionAsync()).Throws<Exception>();

            var controller = new AuthController(passwordMock.Object, tokenMock.Object, unitOfWorkMock.Object,
                roleMock.Object, avatarMock.Object, new Mock<ILogger<AuthController>>().Object);

            var result = await controller.RegisterAsync(model);

            var methodResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, methodResult.StatusCode);
            unitOfWorkMock.Verify(x => x.RollbackTransactionAsync());
            unitOfWorkMock.VerifyAll();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsOk()
        {
            var model = new RegisterDto
                { Email = "Email", Password = "Password", UserName = "userName" };
            var hashPasswordModel = new FormatHashResult { PasswordHash = new byte[3], Salt = new byte[3] };
            byte[] avatarModel = new byte[3];
            var userRoleModel = new Role { Id = Guid.NewGuid(), Name = RoleNames.User };
            var passwordMock = new Mock<IPasswordService>();
            var tokenMock = new Mock<ITokenService>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var roleMock = new Mock<IRoleService>();
            var avatarMock = new Mock<IAvatarService>();

            unitOfWorkMock.Setup(x => x.UserService.GetUserByUserNameAsync(model.UserName)).ReturnsAsync((User?)null);
            unitOfWorkMock.Setup(x => x.UserService.GetUserByEmailAsync(model.Email)).ReturnsAsync((User?)null);
            passwordMock.Setup(x => x.HashPassword(model.Password)).Returns(hashPasswordModel);
            avatarMock.Setup(x => x.GetDefaultUserAvatar(It.IsAny<User>(), 200)).Returns(avatarModel);
            unitOfWorkMock.Setup(x => x.BeginTransactionAsync());
            unitOfWorkMock.Setup(x => x.UserService.AddUserAsync(It.IsAny<User>()));
            roleMock.Setup(x => x.GetRoleByNameAsync(RoleNames.User)).ReturnsAsync(userRoleModel);
            unitOfWorkMock.Setup(x => x.UserRolesService.AddUserToRoleAsync(It.IsAny<Guid>(), It.IsAny<Guid>()));
            unitOfWorkMock.Setup(x => x.CommitTransactionAsync());

            var controller = new AuthController(passwordMock.Object, tokenMock.Object, unitOfWorkMock.Object,
                roleMock.Object, avatarMock.Object, new Mock<ILogger<AuthController>>().Object);

            var result = await controller.RegisterAsync(model);

            Assert.IsType<OkObjectResult>(result);
            unitOfWorkMock.VerifyAll();
        }
    }
}
