using System.Security.Claims;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.Auth;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.Roles_services;
using AccountMicroservice.Api.Services.Token_services;
using AccountMicroservice.Api.Services.User_services;
using AccountMicroservice.Api.Services.User_services.Role_services;
using Microsoft.AspNetCore.Mvc; 

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IPasswordService passwordService, IUserService userService, ITokenService tokenService,
        IUserRolesService userRolesService, IRoleService roleService) : ControllerBase
    {
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
        {
            if (await userService.GetUserByUserNameAsync(model.UserName) != null || await userService.GetUserByEmailAsync(model.Email) != null)
                return Conflict("Current user already exists");

            var hashFormatResult = passwordService.HashPassword(model.Password);

            string passwordHashStr = Convert.ToBase64String(hashFormatResult.PasswordHash);
            string passwordSaltStr = Convert.ToBase64String(hashFormatResult.Salt);

            await userService.AddUserAsync(new User 
            {
                Id = model.Id, Email = model.Email, UserName = model.UserName, IsEmailVerified = false,
                PasswordHash = passwordHashStr, PasswordSalt = passwordSaltStr
            });

            var role = await roleService.GetRoleByNameAsync(RoleNames.User);
            await userRolesService.AddUserToRoleAsync(model.Id, role.Id);

            return Ok();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
        {
            var user = await userService.GetUserByEmailAsync(model.UserNameOrEmail) 
                       ?? await userService.GetUserByUserNameAsync(model.UserNameOrEmail);

            if (user == null)
                return Unauthorized("User is not exist");

            bool checkPassword = passwordService.CheckPassword(Convert.FromBase64String(user.PasswordHash),
                Convert.FromBase64String(user.PasswordSalt), model.Password);

            if (!checkPassword)
                return Unauthorized("Incorrect password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(AdditionalClaimTypes.IsEmailVerified, user.IsEmailVerified.ToString())
            };
            var userRoles = await userRolesService.GetUserRolesAsync(user.Id);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Name));
            }

            string token = tokenService.GenerateAccessToken(claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
            
            await userService.UpdateUserAsync(user);

            return Ok(token);
        }
    }
}
