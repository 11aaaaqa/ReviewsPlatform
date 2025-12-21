using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.User_services;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, IPasswordService passwordService) : ControllerBase
    {
        [HttpGet]
        [Route("get-user-by-id")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Route("get-user-by-user-name")]
        [HttpGet]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            var user = await userService.GetUserByUserNameAsync(userName);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Route("get-user-by-email")]
        [HttpGet]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var user = await userService.GetUserByEmailAsync(email);
            if (user == null) 
                return NotFound();

            return Ok(user);
        }

        [Route("update-user-name")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserNameAsync([FromBody] UpdateUserNameDto model)
        {
            var user = await userService.GetUserByIdAsync(model.UserId);
            if(user == null) return NotFound();

            user.UserName = model.NewUserName;

            try
            {
                await userService.UpdateUserAsync(user);
            }
            catch (Exception e) //заменить на exception, возникающий при конфликте уникальных индексов
            {
                return Conflict("User with current name already exists");
            }

            return Ok();
        }

        [Route("update-user-password")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPasswordAsync([FromBody] UpdateUserPasswordDto model)
        {
            var user = await userService.GetUserByIdAsync(model.UserId);
            if (user == null) return NotFound();
            
            var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
            user.PasswordHash = passwordHashFormatResult.PasswordHash;
            user.PasswordSalt = passwordHashFormatResult.Salt;

            await userService.UpdateUserAsync(user);

            return Ok();
        }
    }
}
