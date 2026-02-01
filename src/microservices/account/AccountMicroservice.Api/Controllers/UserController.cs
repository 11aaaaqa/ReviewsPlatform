using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.DTOs.User;
using AccountMicroservice.Api.Filters.ActionFilters;
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

            return Ok(new
            {
                user.Id, user.UserName, user.Email, user.IsEmailVerified, user.AvatarSource,
                user.RegistrationDate, user.Roles, user.IsAvatarDefault
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get-refresh-token/{userId}")]
        public async Task<IActionResult> GetRefreshToken([FromRoute] Guid userId, [FromQuery] string secret)
        {
            string actualSecret = configuration["InternalEndpoint:Secret"]!;

            if (secret != actualSecret)
                return BadRequest();

            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new { user.RefreshToken });
        }

        [ValidatePassedUserIdActionFilter]
        [Route("update-user-name/{userId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserNameAsync([FromRoute] Guid userId, [FromBody] UpdateUserNameDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if(user == null) return NotFound("User not found");

            string userName = user.UserName;

            if (await unitOfWork.UserService.GetUserByUserNameAsync(model.NewUserName) != null && model.NewUserName.ToLower() != userName.ToLower())
                return Conflict("User with current name already exists");

            user.UserName = model.NewUserName;
            if(user.IsAvatarDefault)
                user.AvatarSource = avatarService.GetDefaultUserAvatar(user);
            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: User {UserId} updated his name from {UserName} to {NewUserName}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), user.Id, userName, model.NewUserName);

            return Ok(tokenService.GenerateAccessToken(tokenService.GetClaims(user)));
        }

        [Route("update-user-password/{userId}")]
        [ValidatePassedUserIdActionFilter]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPasswordAsync(Guid userId, [FromBody] UpdateUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            bool checkPasswordResult = passwordService.CheckPassword(Convert.FromBase64String(user.PasswordHash),
                Convert.FromBase64String(user.PasswordSalt), model.OldPassword);
            if (!checkPasswordResult)
                return BadRequest("Password does not match");

            var passwordHashFormatResult = passwordService.HashPassword(model.NewPassword);
            user.PasswordHash = Convert.ToBase64String(passwordHashFormatResult.PasswordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordHashFormatResult.Salt);
            user.TokenVersion++;
            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

            await unitOfWork.UserService.UpdateUserAsync(user);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("{Timestamp}: User {UserId} updated his password",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), user.Id);
            
            return Ok(tokenService.GenerateAccessToken(tokenService.GetClaims(user)));
        }

        [ValidatePassedUserIdActionFilter]
        [Route("check-password/{userId}")]
        [HttpPost]
        public async Task<IActionResult> CheckUserPassword([FromRoute] Guid userId, [FromBody] CheckUserPasswordDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if(user == null) return NotFound("User not found");

            bool checkPasswordResult = passwordService.CheckPassword(Convert.FromBase64String(user.PasswordHash), 
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
                logger.LogWarning("{Timestamp}: User {UserId} tried to set roles to himself",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), currentUserId);
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
                await unitOfWork.UserService.UpdateUserAsync(user);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogCritical(
                    "{Timestamp}: An exception was thrown while commiting transaction while setting user roles. Exception message: {ExceptionMessage}",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            return Ok();
        }

        [ValidatePassedUserIdActionFilter]
        [RequestSizeLimit(2 * 1024 * 1024)]
        [Route("set-avatar/{userId}")]
        [HttpPut]
        public async Task<IActionResult> SetUserAvatar([FromRoute] Guid userId, [FromBody] SetUserAvatarDto model)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!avatarService.ValidateAvatar(model.AvatarSource))
                return BadRequest("Incorrect file format");

            user.AvatarSource = avatarService.CropCustomUserAvatar(model.AvatarSource);
            user.IsAvatarDefault = false;
            await unitOfWork.UserService.UpdateUserAsync(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [ValidatePassedUserIdActionFilter]
        [Route("reset-avatar/{userId}")]
        [HttpGet]
        public async Task<IActionResult> SetDefaultUserAvatar(Guid userId)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (user.IsAvatarDefault)
                return BadRequest("Avatar is already default");

            user.AvatarSource = avatarService.GetDefaultUserAvatar(user);
            user.IsAvatarDefault = true;
            await unitOfWork.UserService.UpdateUserAsync(user);

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [ValidatePassedUserIdActionFilter]
        [Route("send-email-confirmation-token/{userId}")]
        [HttpPost]
        public async Task<IActionResult> SendEmailConfirmationToken([FromRoute] Guid userId, [FromBody] ConfirmationTokenLink link)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    new { errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });

            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (user.IsEmailVerified)
                return BadRequest("Email is already confirmed");

            var emailConfirmationTokens = await unitOfWork.UserEmailTokenRepository
                .GetEmailTokensByPurposeAsync(userId, EmailTokenPurpose.EmailConfirmation);

            var expiredEmailConfirmationTokens = emailConfirmationTokens
                .Where(x => x.ExpiryTime < DateTime.UtcNow).ToList();
            if (expiredEmailConfirmationTokens.Count > 0)
            {
                await unitOfWork.UserEmailTokenRepository.RemoveRangeAsync(userId, expiredEmailConfirmationTokens);
                await unitOfWork.CompleteAsync();
            }

            if (emailConfirmationTokens.Where(x => x.ExpiryTime > DateTime.UtcNow).ToList().Count >= 3)
            {
                return Conflict("3 emails with active tokens have already been sent");
            }

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
                    "{Timestamp}: User with id {UserId} tried to send an email confirmation link but transaction threw an exception: {ExceptionMessage}",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message});
            }

            logger.LogInformation("{Timestamp}: Email confirmation token sent for user {UserId}",
                DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId);

            return Ok();
        }

        [ValidatePassedUserIdActionFilter]
        [Route("confirm-user-email/{userId}")]
        [HttpGet]
        public async Task<IActionResult> ConfirmUserEmail([FromRoute] Guid userId, [FromQuery] string token)
        {
            var user = await unitOfWork.UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (user.IsEmailVerified)
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
                await unitOfWork.UserService.UpdateUserAsync(user);

                await unitOfWork.UserRolesService.AddUserToRoleAsync(userId, new Guid(RoleIds.VerifiedId));

                await unitOfWork.UserEmailTokenRepository.RemoveAllByPurposeAsync(userId, EmailTokenPurpose.EmailConfirmation);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical("{Timestamp}: User with id {UserId} tried to confirm his email but transaction threw an exception: {ExceptionMessage}",
                    DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { errorMessage = e.Message });
            }

            logger.LogInformation("{Timestamp}: User {UserId} confirmed his email", DateTime.UtcNow.ToString(TimeFormatConstants.DefaultFormat), userId);

            return Ok();
        }
    }
}
