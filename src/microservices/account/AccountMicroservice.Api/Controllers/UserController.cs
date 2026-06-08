using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Services.PasswordServices;
using AccountMicroservice.Api.Services.RolesServices;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.UserServices.AvatarServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using AccountMicroservice.Api.Enums;
using AccountMicroservice.Api.Models.ReturnModels;
using AccountMicroservice.Api.Services.EmailServices;
using AccountMicroservice.Api.Services.TokenServices;

namespace AccountMicroservice.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IPasswordService passwordService, IUnitOfWork unitOfWork, IRoleService roleService, IAvatarService avatarService,
        ILogger<UserController> logger, IConfiguration configuration, ITokenService tokenService, IEmailService emailService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new UserReturnModel
            {
                Id = user.Id, UserName = user.UserName, Email = user.Email, IsEmailVerified = user.IsEmailVerified,
                AvatarSource = user.AvatarSource, RegistrationDate = user.RegistrationDate, Roles = user.Roles,
                IsAvatarDefault = user.IsAvatarDefault
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get-user-by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await unitOfWork.UserService.GetUserByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            return Ok(new UserReturnModel
            {
                Id = user.Id, UserName = user.UserName, Email = user.Email, IsEmailVerified = user.IsEmailVerified,
                AvatarSource = user.AvatarSource, RegistrationDate = user.RegistrationDate, Roles = user.Roles,
                IsAvatarDefault = user.IsAvatarDefault
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("get-users-by-ids")]
        public async Task<IActionResult> GetUserByIds([FromBody] List<Guid> userIds)
            => Ok(await unitOfWork.UserService.GetUsersByUserIds(userIds));

        [AllowAnonymous]
        [HttpGet]
        [Route("get-refresh-token/{userId}")]
        public async Task<IActionResult> GetRefreshToken([FromRoute] Guid userId, [FromQuery] string secret)
        {
            string actualSecret = configuration["INTERNAL_ENDPOINT_SECRET"]!;

            if (secret != actualSecret)
                return BadRequest();

            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new { user.RefreshToken });
        }

        [Route("update-user-name")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserNameAsync([FromBody] UpdateUserNameDto model)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            string userName = user!.UserName;

            if (await unitOfWork.UserService.GetUserByUserNameAsync(model.NewUserName) != null && model.NewUserName.ToLower() != userName.ToLower())
                return Conflict("User with current name already exists");

            user.UserName = model.NewUserName;
            if(user.IsAvatarDefault)
                user.AvatarSource = avatarService.GetDefaultUserAvatar(user);
            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            unitOfWork.UserService.UpdateUser(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("User {UserId} updated his name from {UserName} to {NewUserName}", user.Id, userName, model.NewUserName);

            return Ok(tokenService.GenerateAccessToken(tokenService.GetClaims(user)));
        }

        [Route("check-password")]
        [HttpPost]
        public async Task<IActionResult> CheckUserPassword([FromBody] CheckUserPasswordDto model)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            bool checkPasswordResult = passwordService.CheckPassword(Convert.FromBase64String(user!.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt), model.Password);

            return Ok(checkPasswordResult);
        }

        [Authorize(Roles = RoleNames.Admin)]
        [Route("set-user-roles")]
        [HttpPost]
        public async Task<IActionResult> SetUserRolesAsync([FromBody] SetUserRolesDto model)
        {
            string currentUserIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid currentUserId = new Guid(currentUserIdStr);
            if (currentUserId == model.UserId)
            {
                logger.LogWarning("User {UserId} tried to set roles to himself", currentUserId);
                return BadRequest();
            }

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
                unitOfWork.UserService.UpdateUser(user);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(
                    "An exception was thrown while commiting transaction while setting user roles. Exception message: {ExceptionMessage}", e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            return Ok();
        }

        [RequestSizeLimit(2 * 1024 * 1024)]
        [Route("set-avatar")]
        [HttpPut]
        public async Task<IActionResult> SetUserAvatar([FromBody] SetUserAvatarDto model)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            if (!avatarService.ValidateAvatar(model.AvatarSource))
                return BadRequest("Incorrect file format");

            user!.AvatarSource = avatarService.CropCustomUserAvatar(model.AvatarSource);
            user.IsAvatarDefault = false;
            unitOfWork.UserService.UpdateUser(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Route("reset-avatar")]
        [HttpGet]
        public async Task<IActionResult> SetDefaultUserAvatar()
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            if (user!.IsAvatarDefault)
                return BadRequest("Avatar is already default");

            user.AvatarSource = avatarService.GetDefaultUserAvatar(user);
            user.IsAvatarDefault = true;
            unitOfWork.UserService.UpdateUser(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [Route("send-email-confirmation-token")]
        [HttpPost]
        public async Task<IActionResult> SendEmailConfirmationToken([FromBody] ConfirmationTokenLink link)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            if (user!.IsEmailVerified)
                return BadRequest("Email is already confirmed");

            var emailConfirmationTokens = await unitOfWork.UserEmailTokenRepository
                .GetEmailTokensByPurposeAsync(userId, EmailTokenPurpose.EmailConfirmation);

            if (emailConfirmationTokens.Where(x => x.ExpiryTime > DateTime.UtcNow).ToList().Count >= 3)
                return Conflict("3 emails with active tokens have already been sent");
            
            try
            {
                await unitOfWork.BeginTransactionAsync();

                string token = await unitOfWork.UserEmailTokenRepository.AddAsync(userId, EmailTokenPurpose.EmailConfirmation);

                await unitOfWork.CompleteAsync();

                string emailBody = $"<div><p style=\"margin: 0 0 10px 0;\">Подтвердите Ваш адрес электронной почты, перейдя по <a href=\"{link.Url}{token}\">ссылке</a>.</p><p style=\"margin: 0;\">Если Вы не запрашивали подтверждение, проигнорируйте это письмо.</p></div>";
                await emailService.SendEmailAsync(user.Email, "Подтверждение почты", emailBody);

                await unitOfWork.CommitTransactionAsync();
            }
            catch(Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(
                    "User with id {UserId} tried to send an email confirmation link but transaction threw an exception: {ExceptionMessage}", 
                    userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message});
            }

            logger.LogInformation("Email confirmation token sent for user {UserId}", userId);

            return Ok();
        }

        [Route("confirm-user-email")]
        [HttpGet]
        public async Task<IActionResult> ConfirmUserEmail([FromQuery] string token)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);

            if (user!.IsEmailVerified)
                return Conflict("Email is already verified");

            var emailToken = await unitOfWork.UserEmailTokenRepository.GetAsync(userId, token, EmailTokenPurpose.EmailConfirmation);
            if (emailToken == null)
                return NotFound("Token not found");

            if (emailToken.ExpiryTime < DateTime.UtcNow)
                return BadRequest("Token has expired");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                user.IsEmailVerified = true;
                user.RefreshToken = tokenService.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
                unitOfWork.UserService.UpdateUser(user);

                await unitOfWork.UserRolesService.AddUserToRoleAsync(userId, new Guid(RoleIds.VerifiedId));

                await unitOfWork.UserEmailTokenRepository.RemoveAllByPurposeAsync(userId, EmailTokenPurpose.EmailConfirmation);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical("User with id {UserId} tried to confirm his email but transaction threw an exception: {ExceptionMessage}", 
                    userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            logger.LogInformation("User {UserId} confirmed his email", userId);

            user.Roles.Add(new Role { Id = new Guid(RoleIds.VerifiedId), Name = RoleNames.Verified });
            return Ok(tokenService.GenerateAccessToken(tokenService.GetClaims(user)));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("send-password-reset-token/{userId}")]
        public async Task<IActionResult> SendPasswordResetToken(Guid userId, [FromBody] ConfirmationTokenLink link)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            if (!user.IsEmailVerified) return BadRequest("Email is not verified");

            var passwordResetEmailTokens = await unitOfWork.UserEmailTokenRepository
                .GetEmailTokensByPurposeAsync(userId, EmailTokenPurpose.PasswordReset);
            
            if (passwordResetEmailTokens.Any(x => x.ExpiryTime > DateTime.UtcNow))
                return Conflict("Password reset has already been requested");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                string token = await unitOfWork.UserEmailTokenRepository.AddAsync(userId, EmailTokenPurpose.PasswordReset);
                await unitOfWork.CompleteAsync();

                string emailBody = $"<div><p style=\"margin: 0 0 10px 0;\">Для смены Вашего пароля перейдите по <a href=\"{link.Url}{token}\">ссылке</a>.</p><p style=\"margin: 0;\">Если Вы не запрашивали смену пароля, проигнорируйте это письмо.</p></div>";
                await emailService.SendEmailAsync(user.Email, "Сброс пароля", emailBody);

                await unitOfWork.CommitTransactionAsync();
            }
            catch(Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical("Password reset was tried to be requested for user {UserId} but transaction threw an exception: {ErrorMessage}",
                    userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("update-user-password/{userId}")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserPasswordAsync(Guid userId, [FromBody] UpdateUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var emailToken = await unitOfWork.UserEmailTokenRepository
                .GetAsync(userId, model.Token, EmailTokenPurpose.PasswordReset);
            if (emailToken == null)
                return NotFound("Token not found");

            if (emailToken.ExpiryTime < DateTime.UtcNow)
                return BadRequest("Token has expired");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
                user.PasswordHash = Convert.ToBase64String(passwordHashFormatResult.PasswordHash);
                user.PasswordSalt = Convert.ToBase64String(passwordHashFormatResult.Salt);
                user.TokenVersion++;
                user.RefreshToken = tokenService.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
                unitOfWork.UserService.UpdateUser(user);

                await unitOfWork.UserEmailTokenRepository.RemoveAllByPurposeAsync(userId, EmailTokenPurpose.PasswordReset);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical("User {UserId} tried to update his password but transaction threw an exception: {ErrorMessage}", userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            logger.LogInformation("User {UserId} updated his password", user.Id);

            return Ok();
        }
    }
}
