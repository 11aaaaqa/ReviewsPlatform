using System.Net;
using System.Security.Claims;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.Auth;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.TokenServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IPasswordService passwordService, ITokenService tokenService, IUnitOfWork unitOfWork,
        IRoleService roleService, IAvatarService avatarService, ILogger<AuthController> logger) : ControllerBase
    {
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
        {
            if (await unitOfWork.UserService.GetUserByUserNameAsync(model.UserName) != null
                || await unitOfWork.UserService.GetUserByEmailAsync(model.Email) != null)
                return Conflict("Current user already exists");

            var hashFormatResult = passwordService.HashPassword(model.Password);

            string passwordHashStr = Convert.ToBase64String(hashFormatResult.PasswordHash);
            string passwordSaltStr = Convert.ToBase64String(hashFormatResult.Salt);

            User userToAdd = new User
            {
                Id = Guid.NewGuid(), Email = model.Email, UserName = model.UserName, IsEmailVerified = false,
                PasswordHash = passwordHashStr, PasswordSalt = passwordSaltStr, 
                RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            userToAdd.AvatarSource = avatarService.GetDefaultUserAvatar(userToAdd);
            userToAdd.IsAvatarDefault = true;

            try
            {
                await unitOfWork.BeginTransactionAsync();

                await unitOfWork.UserService.AddUserAsync(userToAdd);

                var role = await roleService.GetRoleByNameAsync(RoleNames.User);
                await unitOfWork.UserRolesService.AddUserToRoleAsync(userToAdd.Id, role.Id);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical("User have not been registered due to server error: {Exception}", ex);
                await unitOfWork.RollbackTransactionAsync();
                return StatusCode((int)HttpStatusCode.InternalServerError, new {errorMessage = ex.Message});
            }

            logger.LogInformation("User {UserId} with {Email} email successfully registered", userToAdd.Id, userToAdd.Email);

            return Ok(userToAdd.Id);
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
        {
            var user = await unitOfWork.UserService.GetUserByEmailAsync(model.UserNameOrEmail) 
                       ?? await unitOfWork.UserService.GetUserByUserNameAsync(model.UserNameOrEmail);

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
            var userRoles = user.Roles;
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Name));
            }

            string token = tokenService.GenerateAccessToken(claims);

            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
            
            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("User {UserId} logged in", user.Id);

            return Ok(token);
        }
    }
}
