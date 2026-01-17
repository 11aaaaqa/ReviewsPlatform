using System.Net;
using System.Text;
using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IPasswordService passwordService, IUnitOfWork unitOfWork, IRoleService roleService,
        ILogger<UserController> logger) : ControllerBase
    {
        [HttpGet]
        [Route("get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Route("update-user-name")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserNameAsync([FromBody] UpdateUserNameDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(model.UserId);
            if(user == null) return NotFound();

            string userName = user.UserName;

            if (await unitOfWork.UserService.GetUserByUserNameAsync(model.NewUserName) != null)
                return Conflict("User with current name already exists");

            user.UserName = model.NewUserName;
            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("User {UserId} updated his name from {UserName} to {NewUserName}", user.Id, userName, model.NewUserName);

            return Ok();
        }

        [Route("update-user-password")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPasswordAsync([FromBody] UpdateUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(model.UserId);
            if (user == null) return NotFound();
            
            var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
            user.PasswordHash = Encoding.UTF8.GetString(passwordHashFormatResult.PasswordHash);
            user.PasswordSalt = Encoding.UTF8.GetString(passwordHashFormatResult.Salt);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("User {UserId} updated his password", user.Id);

            return Ok();
        }

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

            using var memoryStream = new MemoryStream(model.AvatarSource);
            using var codec = SKCodec.Create(memoryStream);
            var format = codec.EncodedFormat;
            if (format != SKEncodedImageFormat.Png && format != SKEncodedImageFormat.Jpeg)
            {
                return BadRequest("Incorrect file format");
            }

            using SKBitmap bitmap = SKBitmap.Decode(model.AvatarSource);
            int size = Math.Min(bitmap.Height, bitmap.Width);
            using SKBitmap editedBitmap = new SKBitmap(size, size);

            using SKCanvas canvas = new SKCanvas(editedBitmap);
            using SKPath path = new SKPath();

            canvas.Clear(SKColors.Transparent);
            path.AddCircle(size / 2f, size / 2f, size / 2f);
            canvas.ClipPath(path);
            canvas.DrawBitmap(bitmap, (size - bitmap.Width) / 2f, (size - bitmap.Height) / 2f);

            using SKImage editedImage = SKImage.FromBitmap(editedBitmap);
            using SKData data = editedImage.Encode(SKEncodedImageFormat.Png, 100);

            byte[] avatarSource = data.ToArray();
            user.AvatarSource = avatarSource;
            await unitOfWork.UserService.UpdateUserAsync(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }
    }
}
