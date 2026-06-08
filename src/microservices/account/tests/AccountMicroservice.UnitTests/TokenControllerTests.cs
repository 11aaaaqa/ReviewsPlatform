using System.Security.Claims;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.Controllers;
using AccountMicroservice.Api.DTOs.Token;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccountMicroservice.UnitTests
{
    public class TokenControllerTests
    {
        [Fact]
        public async Task RevokeAsync_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var mock = new Mock<IUnitOfWork>();
            var userModel = new User
            {
                Id = userId, TokenVersion = 0, RefreshToken = "RefreshToken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(1)
            };
            mock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(userModel);
            mock.Setup(x => x.UserService.UpdateUser(userModel));
            mock.Setup(x => x.CompleteAsync());
            var controller = new TokenController(mock.Object, new Mock<ITokenService>().Object,
                new Mock<ILogger<TokenController>>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.RevokeAsync();

            Assert.IsType<OkResult>(result);
            mock.VerifyAll();
        }

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorizedUserNull()
        {
            var model = new RefreshTokenDto { RefreshToken = "RefreshToken", AccessToken = "AccessToken" };
            Guid userId = Guid.NewGuid();
            int tokenVersion = 1;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(AdditionalClaimTypes.TokenVersion, tokenVersion.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var tokenMock = new Mock<ITokenService>();
            var uowMock = new Mock<IUnitOfWork>();
            tokenMock.Setup(x => x.GetPrincipalFromToken(model.AccessToken)).Returns(principal);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);
            var controller = new TokenController(uowMock.Object, tokenMock.Object,
                new Mock<ILogger<TokenController>>().Object);

            var result = await controller.RefreshAsync(model);

            Assert.IsType<UnauthorizedResult>(result);
            tokenMock.Verify(x => x.GetPrincipalFromToken(model.AccessToken));
            tokenMock.VerifyAll();
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.VerifyAll();
        }

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorizedRefreshTokenDoesNotMatch()
        {
            var model = new RefreshTokenDto { RefreshToken = "RefreshToken", AccessToken = "AccessToken" };
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(5);
            string invalidRefreshToken = "InvalidRefreshToken";
            Guid userId = Guid.NewGuid();
            int tokenVersion = 1;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(AdditionalClaimTypes.TokenVersion, tokenVersion.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new User
            {
                Id = userId, TokenVersion = tokenVersion, RefreshToken = invalidRefreshToken,
                RefreshTokenExpiryTime = expiryTime
            };
            var tokenMock = new Mock<ITokenService>();
            var uowMock = new Mock<IUnitOfWork>();
            tokenMock.Setup(x => x.GetPrincipalFromToken(model.AccessToken)).Returns(principal);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            var controller = new TokenController(uowMock.Object, tokenMock.Object,
                new Mock<ILogger<TokenController>>().Object);

            var result = await controller.RefreshAsync(model);

            Assert.IsType<UnauthorizedResult>(result);
            tokenMock.Verify(x => x.GetPrincipalFromToken(model.AccessToken));
            tokenMock.VerifyAll();
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.VerifyAll();
        }

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorizedRefreshTokenExpired()
        {
            var model = new RefreshTokenDto { RefreshToken = "RefreshToken", AccessToken = "AccessToken" };
            DateTime invalidRefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-5);
            Guid userId = Guid.NewGuid();
            int tokenVersion = 1;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(AdditionalClaimTypes.TokenVersion, tokenVersion.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new User { Id = userId, TokenVersion = tokenVersion, RefreshToken = model.RefreshToken,
                RefreshTokenExpiryTime = invalidRefreshTokenExpiryTime};
            var tokenMock = new Mock<ITokenService>();
            var uowMock = new Mock<IUnitOfWork>();
            tokenMock.Setup(x => x.GetPrincipalFromToken(model.AccessToken)).Returns(principal);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            var controller = new TokenController(uowMock.Object, tokenMock.Object,
                new Mock<ILogger<TokenController>>().Object);

            var result = await controller.RefreshAsync(model);

            Assert.IsType<UnauthorizedResult>(result);
            tokenMock.Verify(x => x.GetPrincipalFromToken(model.AccessToken));
            tokenMock.VerifyAll();
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.VerifyAll();
        }

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorizedTokenVersionDoesNotMatch()
        {
            var model = new RefreshTokenDto { RefreshToken = "RefreshToken", AccessToken = "AccessToken" };
            DateTime refreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(5);
            Guid userId = Guid.NewGuid();
            int tokenVersion = 1;
            int invalidTokenVersion = 0;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(AdditionalClaimTypes.TokenVersion, tokenVersion.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new User
            {
                Id = userId,
                TokenVersion = invalidTokenVersion,
                RefreshToken = model.RefreshToken,
                RefreshTokenExpiryTime = refreshTokenExpiryTime
            };
            var tokenMock = new Mock<ITokenService>();
            var uowMock = new Mock<IUnitOfWork>();
            tokenMock.Setup(x => x.GetPrincipalFromToken(model.AccessToken)).Returns(principal);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            var controller = new TokenController(uowMock.Object, tokenMock.Object,
                new Mock<ILogger<TokenController>>().Object);

            var result = await controller.RefreshAsync(model);

            Assert.IsType<UnauthorizedResult>(result);
            tokenMock.Verify(x => x.GetPrincipalFromToken(model.AccessToken));
            tokenMock.VerifyAll();
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.VerifyAll();
        }

        [Fact]
        public async Task RefreshAsync_ReturnsOk()
        {
            var model = new RefreshTokenDto { RefreshToken = "RefreshToken", AccessToken = "AccessToken" };
            DateTime refreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(5);
            Guid userId = Guid.NewGuid();
            int tokenVersion = 1;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(AdditionalClaimTypes.TokenVersion, tokenVersion.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new User
            {
                Id = userId,
                TokenVersion = tokenVersion,
                RefreshToken = model.RefreshToken,
                RefreshTokenExpiryTime = refreshTokenExpiryTime
            };
            string renewedAccessToken = "RenewedAccessToken";
            var tokenMock = new Mock<ITokenService>();
            var uowMock = new Mock<IUnitOfWork>();
            tokenMock.Setup(x => x.GetPrincipalFromToken(model.AccessToken)).Returns(principal);
            uowMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            tokenMock.Setup(x => x.GetClaims(user)).Returns(It.IsAny<List<Claim>>());
            tokenMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns(renewedAccessToken);
            uowMock.Setup(x => x.UserService.UpdateUser(user));
            uowMock.Setup(x => x.CompleteAsync());
            var controller = new TokenController(uowMock.Object, tokenMock.Object,
                new Mock<ILogger<TokenController>>().Object);

            var result = await controller.RefreshAsync(model);

            var methodResult = Assert.IsType<OkObjectResult>(result);
            var returnedAccessToken = Assert.IsType<string>(methodResult.Value);
            Assert.Equal(renewedAccessToken, returnedAccessToken);
            tokenMock.Verify(x => x.GetPrincipalFromToken(model.AccessToken));
            tokenMock.Verify(x => x.GetClaims(user));
            tokenMock.Verify(x => x.GenerateAccessToken(It.IsAny<List<Claim>>()));
            tokenMock.VerifyAll();
            uowMock.Verify(x => x.UserService.GetUserByIdAsync(userId));
            uowMock.Verify(x => x.UserService.UpdateUser(user));
            uowMock.Verify(x => x.CompleteAsync());
            uowMock.VerifyAll();
        }
    }
}
