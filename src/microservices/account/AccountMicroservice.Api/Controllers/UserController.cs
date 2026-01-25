using System.Net;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IPasswordService passwordService, IUnitOfWork unitOfWork, IRoleService roleService, IAvatarService avatarService,
        ILogger<UserController> logger) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id, user.UserName, user.Email, user.IsEmailVerified, user.AvatarSource, user.RegistrationDate,
                user.RefreshToken, user.RefreshTokenExpiryTime, user.Roles, user.IsAvatarDefault
            });
        }

        [Route("update-user-name")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserNameAsync([FromBody] UpdateUserNameDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(model.UserId);
            if(user == null) return NotFound();

            string userName = user.UserName;

            if (await unitOfWork.UserService.GetUserByUserNameAsync(model.NewUserName) != null && model.NewUserName.ToLower() != userName.ToLower())
                return Conflict("User with current name already exists");

            user.UserName = model.NewUserName;
            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: User {UserId} updated his name from {UserName} to {NewUserName}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), user.Id, userName, model.NewUserName);

            return Ok();
        }

        [Route("update-user-password/{userId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPasswordAsync(Guid userId, [FromBody] UpdateUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            
            var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
            user.PasswordHash = Convert.ToBase64String(passwordHashFormatResult.PasswordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordHashFormatResult.Salt);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: User {UserId} updated his password",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), user.Id);

            return Ok();
        }

        [Route("check-password/{userId}")]
        [HttpPost]
        public async Task<IActionResult> CheckUserPassword([FromRoute] Guid userId, [FromBody] CheckUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if(user == null) return NotFound();

            bool checkPasswordResult = passwordService.CheckPassword(Convert.FromBase64String(user.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt), model.Password);

            return Ok(checkPasswordResult);
        }

        [Authorize(Roles = RoleNames.Admin)]
        [Route("set-user-roles")]
        [HttpPost]
        public async Task<IActionResult> SetUserRolesAsync([FromBody] SetUserRolesDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(model.UserId);
            if (user == null)
                return NotFound("User not found");

            model.RoleIds = model.RoleIds.Distinct().ToList();

            List<Role> allRoles = await roleService.GetAllRolesAsync();
            List<Guid> allRoleIds = allRoles.Select(x => x.Id).ToList();
            if (model.RoleIds.Except(allRoleIds).Any())
                return BadRequest("Role with current id does not exist");

            var currentUserRoles = user.Roles;

            try
            {
                await unitOfWork.BeginTransactionAsync();

                if (currentUserRoles.Count == 0)
                {
                    await unitOfWork.UserRolesService.AddUserToRolesRangeAsync(model.UserId, model.RoleIds);
                }
                else
                {
                    List<Guid> currentUserRoleIds = currentUserRoles.Select(x => x.Id).ToList();
                    List<Guid> userRoleIdsToDelete = currentUserRoleIds.Except(model.RoleIds).ToList();
                    List<Guid> userRoleIdsToAdd = model.RoleIds.Except(currentUserRoleIds).ToList();
                    await unitOfWork.UserRolesService.AddUserToRolesRangeAsync(model.UserId, userRoleIdsToAdd);
                    await unitOfWork.UserRolesService.RemoveUserRolesRangeAsync(model.UserId, userRoleIdsToDelete);
                }

                user.IsEmailVerified = model.RoleIds.Any(x => x == new Guid(RoleIds.VerifiedId));
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = new DateTime();
                await unitOfWork.UserService.UpdateUserAsync(user);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            return Ok();
        }

        [RequestSizeLimit(2 * 1024 * 1024)]
        [Route("set-avatar")]
        [HttpPut]
        public async Task<IActionResult> SetUserAvatar([FromBody] SetUserAvatarDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            if(!avatarService.ValidateAvatar(model.AvatarSource))
                return BadRequest("Incorrect file format");

            user.AvatarSource = avatarService.CropCustomUserAvatar(model.AvatarSource);
            user.IsAvatarDefault = false;
            await unitOfWork.UserService.UpdateUserAsync(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Route("reset-avatar/{userId}")]
        [HttpGet]
        public async Task<IActionResult> SetDefaultUserAvatar(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (user.IsAvatarDefault)
                return BadRequest("Avatar is already default");

            user.AvatarSource = avatarService.GetDefaultUserAvatar(user);
            user.IsAvatarDefault = true;
            await unitOfWork.UserService.UpdateUserAsync(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }
    }
}
