using System.Security.Claims;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.Token;
using AccountMicroservice.Api.Filters.ActionFilters;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(IUnitOfWork unitOfWork, ITokenService tokenService, ILogger<TokenController> logger) : ControllerBase
    {
        [Authorize]
        [ValidatePassedUserIdActionFilter]
        [Route("revoke/{userId}")]
        [HttpGet]
        public async Task<IActionResult> RevokeAsync(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            user.RefreshTokenExpiryTime = new DateTime();
            user.RefreshToken = null;
            user.TokenVersion++;

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: User {UserId} logged out",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId);

            return Ok();
        }

        [Route("refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenDto model)
        {
            var principal = tokenService.GetPrincipalFromToken(model.AccessToken);
            string userIdStr = principal.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            int tokenVersion = int.Parse(principal.Claims.Single(x => x.Type == AdditionalClaimTypes.TokenVersion).Value);

            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow || tokenVersion != user.TokenVersion)
                return Unauthorized();

            List<Claim> claims = tokenService.GetClaims(user);
            string accessToken = tokenService.GenerateAccessToken(claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: Token for user {UserId} refreshed",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), user.Id);

            return Ok(accessToken);
        }
    }
}
