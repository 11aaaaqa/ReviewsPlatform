using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestrictionMicroservice.Api.Constants;
using RestrictionMicroservice.Api.DTOs.restriction;
using RestrictionMicroservice.Api.Models;
using RestrictionMicroservice.Api.Models.Business;
using RestrictionMicroservice.Api.Services.UnitOfWork;
using System.Security.Claims;

namespace RestrictionMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
    public class RestrictionController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpGet]
        [Route("get-by-id/{restrictionId}")]
        public async Task<IActionResult> GetRestrictionByIdAsync([FromRoute] Guid restrictionId)
        {
            var restriction = await unitOfWork.RestrictionRepository.GetByIdAsync(restrictionId);
            if (restriction == null)
                return NotFound("Restriction with current identifier does not exist");

            return Ok(restriction);
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllRestrictionsAsync([FromQuery] Pagination pagination)
        {
            var restrictions = await unitOfWork.RestrictionRepository.GetAllAsync(pagination.PageNumber, pagination.PageSize);

            var restrictionsNextPage = await unitOfWork.RestrictionRepository.GetAllAsync(pagination.PageNumber + 1, pagination.PageSize);
            bool isNextPageExisted = restrictionsNextPage.Count > 0;

            return Ok(new RestrictionsResult { Restrictions = restrictions, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("find")]
        public async Task<IActionResult> FindRestrictionsAsync(string query, [FromQuery] Pagination pagination)
        {
            var restrictions = await unitOfWork.RestrictionRepository.GetAllAsync(query, pagination.PageNumber, pagination.PageSize);

            var restrictionsNextPage = await unitOfWork.RestrictionRepository.GetAllAsync(query, pagination.PageNumber + 1, pagination.PageSize);
            bool isNextPageExisted = restrictionsNextPage.Count > 0;

            return Ok(new RestrictionsResult { Restrictions = restrictions, IsNextPageExisted = isNextPageExisted });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get-active/{userId}")]
        public async Task<IActionResult> GetActiveRestrictionByUserIdAsync([FromRoute] Guid userId)
        {
            var restriction = await unitOfWork.RestrictionRepository.GetActiveRestrictionByRestrictedUserIdAsync(userId);
            if (restriction == null)
                return NotFound("User with current identifier does not have any restrictions");

            return Ok(restriction);
        }

        [HttpGet]
        [Route("get-by-restricted-user/{userId}")]
        public async Task<IActionResult> GetAllRestrictionsByRestrictedUserIdAsync([FromRoute] Guid userId, [FromQuery] Pagination pagination)
        {
            var restrictions =
                await unitOfWork.RestrictionRepository.GetAllByRestrictedUserIdAsync(userId, pagination.PageNumber, pagination.PageSize);

            var restrictionsNextPage =
                await unitOfWork.RestrictionRepository.GetAllByRestrictedUserIdAsync(userId, pagination.PageNumber + 1, pagination.PageSize);
            bool isNextPageExisted = restrictionsNextPage.Count > 0;

            return Ok(new RestrictionsResult { Restrictions = restrictions, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("get-by-restricting-user/{userId}")]
        public async Task<IActionResult> GetAllRestrictionsByRestrictingUserIdAsync([FromRoute] Guid userId, [FromQuery] Pagination pagination)
        {
            var restrictions =
                await unitOfWork.RestrictionRepository.GetAllByRestrictingUserIdAsync(userId, pagination.PageNumber, pagination.PageSize);

            var restrictionsNextPage =
                await unitOfWork.RestrictionRepository.GetAllByRestrictingUserIdAsync(userId, pagination.PageNumber + 1, pagination.PageSize);
            bool isNextPageExisted = restrictionsNextPage.Count > 0;

            return Ok(new RestrictionsResult { Restrictions = restrictions, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("find-by-restricting-user/{userId}")]
        public async Task<IActionResult> FindRestrictionsByRestrictingUserIdAsync([FromRoute] Guid userId, string query,
            [FromQuery] Pagination pagination)
        {
            var restrictions =
                await unitOfWork.RestrictionRepository.GetAllByRestrictingUserIdAsync(query, userId, pagination.PageNumber, pagination.PageSize);

            var restrictionsNextPage =
                await unitOfWork.RestrictionRepository.GetAllByRestrictingUserIdAsync(query, userId, pagination.PageNumber + 1, pagination.PageSize);
            bool isNextPageExisted = restrictionsNextPage.Count > 0;

            return Ok(new RestrictionsResult { Restrictions = restrictions, IsNextPageExisted = isNextPageExisted });
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddRestrictionAsync([FromBody] AddRestrictionDto model)
        {
            var activeRestriction = await unitOfWork.RestrictionRepository.GetActiveRestrictionByRestrictedUserIdAsync(model.RestrictedUserId);
            if (activeRestriction != null)
                return Conflict("User already has an active restriction");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);
            var restrictionToAdd = new Restriction
            {
                Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, IsPermanent = model.IsPermanent,
                Reason = model.Reason, RestrictingUserId = userId,
                RestrictedUserId = model.RestrictedUserId, RestrictionType = model.RestrictionType,
                ExpiryTime = model.IsPermanent ? DateTime.UtcNow : DateTime.UtcNow.Add(model.Duration),

                IsDisabled = false, DisablingReason = null, DisabledAt = new DateTime(), DisabledByUserId = Guid.Empty
            };
            await unitOfWork.RestrictionRepository.AddAsync(restrictionToAdd);
            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpPut]
        [Route("disable")]
        public async Task<IActionResult> DisableRestrictionAsync([FromBody] DisableRestrictionDto model)
        {
            var restriction = await unitOfWork.RestrictionRepository.GetByIdAsync(model.RestrictionId);
            if (restriction == null)
                return NotFound("Restriction with current identifier does not exist");

            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            restriction.IsDisabled = true;
            restriction.DisabledByUserId = userId;
            restriction.DisabledAt = DateTime.UtcNow;
            restriction.DisablingReason = model.DisablingReason;

            unitOfWork.RestrictionRepository.Update(restriction);
            await unitOfWork.CompleteAsync();

            return Ok();
        }
    }
}
