using System.Text;
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
        [Route("get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user = await userService.GetUserByIdAsync(userId);
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

            if (await userService.GetUserByUserNameAsync(model.NewUserName) != null)
                return Conflict("User with current name already exists");

            user.UserName = model.NewUserName;
            await userService.UpdateUserAsync(user);

            return Ok();
        }

        [Route("update-user-password")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPasswordAsync([FromBody] UpdateUserPasswordDto model)
        {
            var user = await userService.GetUserByIdAsync(model.UserId);
            if (user == null) return NotFound();
            
            var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
            user.PasswordHash = Encoding.UTF8.GetString(passwordHashFormatResult.PasswordHash);
            user.PasswordSalt = Encoding.UTF8.GetString(passwordHashFormatResult.Salt);

            await userService.UpdateUserAsync(user);

            return Ok();
        }
    }
}
