using AccountMicroservice.Api.DTOs.Token;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(IUnitOfWork unitOfWork, ITokenService tokenService) : ControllerBase
    {
        [Route("revoke/{userId}")]
        [HttpGet]
        public async Task<IActionResult> RevokeAsync(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            user.RefreshTokenExpiryTime = new DateTime();
            user.RefreshToken = null;

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Route("refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenDto model)
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            string userName = principal.Identity.Name;

            var user = await unitOfWork.UserService.GetUserByUserNameAsync(userName);

            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized();

            string accessToken = tokenService.GenerateAccessToken(principal.Claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            return Ok(accessToken);
        }
    }
}
