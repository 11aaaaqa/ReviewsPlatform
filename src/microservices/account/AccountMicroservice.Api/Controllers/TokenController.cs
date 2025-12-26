using AccountMicroservice.Api.DTOs.Token;
using AccountMicroservice.Api.Services.Token_services;
using AccountMicroservice.Api.Services.User_services;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(IUserService userService, ITokenService tokenService) : ControllerBase
    {
        [Route("revoke")]
        [HttpGet]
        public async Task<IActionResult> RevokeAsync(string userEmail)
        {
            var user = await userService.GetUserByEmailAsync(userEmail);
            if (user == null) return NotFound();

            user.RefreshTokenExpiryTime = new DateTime();
            user.RefreshToken = null;

            await userService.UpdateUserAsync(user);

            return Ok();
        }

        [Route("refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenDto model)
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            string userName = principal.Identity.Name;

            var user = await userService.GetUserByUserNameAsync(userName);

            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized();

            string accessToken = tokenService.GenerateAccessToken(principal.Claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            await userService.UpdateUserAsync(user);

            return Ok(accessToken);
        }
    }
}
