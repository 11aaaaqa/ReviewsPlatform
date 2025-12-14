using System.Security.Claims;
using AccountMicroservice.Api.DTOs.Auth;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.Token_services;
using AccountMicroservice.Api.Services.User_services;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IPasswordService passwordService, IUserService userService, ITokenService tokenService) : ControllerBase
    {
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
        {
            var hashFormatResult = passwordService.HashPassword(model.Password);

            try
            {
                await userService.AddUserAsync(new User
                {
                    Id = model.Id,
                    Email = model.Email,
                    UserName = model.UserName,
                    IsEmailVerified = false,
                    PasswordHash = hashFormatResult.PasswordHash,
                    PasswordSalt = hashFormatResult.Salt
                });
            }
            catch (Exception e) //заменить на exception, возникающий при конфликте уникальных индексов
            {
                return Conflict("Current user already exists");
            }

            return Ok();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
        {
            var user = await userService.GetUserByEmailAsync(model.UserNameOrEmail) 
                       ?? await userService.GetUserByUserNameAsync(model.UserNameOrEmail);

            bool checkPassword = passwordService.CheckPassword(user.PasswordHash, user.PasswordSalt, model.Password);

            if (!checkPassword)
                return Unauthorized("Incorrect password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var token = tokenService.GenerateAccessToken(claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
            
            await userService.UpdateUserAsync(user);

            return Ok(token);
        }
    }
}
