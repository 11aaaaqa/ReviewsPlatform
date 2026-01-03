using System.Text;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.Roles_services;
using AccountMicroservice.Api.Services.User_services;
using AccountMicroservice.Api.Services.User_services.Role_services;
using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, IPasswordService passwordService, IUserRolesService userRoleService,
        IRoleService roleService) : ControllerBase
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

        [Route("set-user-roles")]
        [HttpPost]
        public async Task<IActionResult> SetUserRolesAsync([FromBody] SetUserRolesDto model)
        {
            var user = await userService.GetUserByIdAsync(model.UserId);
            if (user == null)
                return NotFound("User not found");

            model.RoleIds = model.RoleIds.Distinct().ToList();

            List<Role> allRoles = await roleService.GetAllRolesAsync();
            List<Guid> allRoleIds = allRoles.Select(x => x.Id).ToList();
            if (model.RoleIds.Except(allRoleIds).Any())
                return BadRequest("Role with current id does not exist");

            var currentUserRoles = await userRoleService.GetUserRolesAsync(model.UserId);
            if (currentUserRoles.Count == 0)
            {
                await userRoleService.AddUserToRolesRangeAsync(model.UserId, model.RoleIds);
            }
            else
            {
                List<Guid> currentUserRoleIds = currentUserRoles.Select(x => x.Id).ToList();
                List<Guid> userRoleIdsToDelete = currentUserRoleIds.Except(model.RoleIds).ToList();
                List<Guid> userRoleIdsToAdd = model.RoleIds.Except(currentUserRoleIds).ToList();
                await userRoleService.AddUserToRolesRangeAsync(model.UserId, userRoleIdsToAdd);
                await userRoleService.RemoveUserRolesRangeAsync(model.UserId, userRoleIdsToDelete);
            }

            return Ok();
        }
    }
}
