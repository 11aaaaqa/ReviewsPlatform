using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestrictionMicroservice.Api.Constants;
using RestrictionMicroservice.Api.DTOs.report;
using RestrictionMicroservice.Api.Models.Business;
using RestrictionMicroservice.Api.Services.UnitOfWork;

namespace RestrictionMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Moderator)]
    public class ReportController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpGet]
        [Route("get-by-id/{reportId}")]
        public async Task<IActionResult> GetReportByIdAsync([FromRoute] Guid reportId)
        {
            var report = await unitOfWork.ReportRepository.GetByIdAsync(reportId);
            if (report == null)
                return NotFound("Report with current identifier does not exist");

            return Ok(report);
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllReportsAsync(int pageNumber, int pageSize)
        {
            var reports = await unitOfWork.ReportRepository.GetAllAsync(pageSize, pageNumber);

            var reportsNextPage = await unitOfWork.ReportRepository.GetAllAsync(pageSize, pageNumber + 1);
            bool isNextPageExisted = reportsNextPage.Count > 0;

            return Ok(new ReportsResult { Reports = reports, IsNextPageExisted = isNextPageExisted });
        }

        [HttpGet]
        [Route("get-by-user/{userId}")]
        public async Task<IActionResult> GetAllReportsByUserIdAsync([FromRoute] Guid userId, int pageNumber, int pageSize)
        {
            var reports = await unitOfWork.ReportRepository.GetAllAsync(userId, pageSize, pageNumber);

            var reportsNextPage = await unitOfWork.ReportRepository.GetAllAsync(userId, pageSize, pageNumber + 1);
            bool isNextPageExisted = reportsNextPage.Count > 0;

            return Ok(new ReportsResult { Reports = reports, IsNextPageExisted = isNextPageExisted });
        }

        [Authorize]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddReportAsync([FromBody] AddReportDto model)
        {
            string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Guid userId = new Guid(userIdStr);

            var report = new Report
            {
                Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Reason = model.Reason,
                ReportOnEntityId = model.ReportOnEntityId, ReportType = model.ReportType,
                ReportedUserId = model.ReportedUserId, ReportingUserId = userId
            };

            await unitOfWork.ReportRepository.AddAsync(report);
            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("remove/{reportId}")]
        public async Task<IActionResult> RemoveReportAsync([FromRoute] Guid reportId)
        {
            try
            {
                await unitOfWork.ReportRepository.RemoveAsync(reportId);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }

            await unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("remove-all-by-reporting-user/{userId}")]
        public async Task<IActionResult> RemoveAllReportsByReportingUserIdAsync([FromRoute] Guid userId)
        {
            await unitOfWork.ReportRepository.RemoveAllByReportingUserIdAsync(userId);
            await unitOfWork.CompleteAsync();

            return Ok();
        }
    }
}
