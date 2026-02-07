using AccountMicroservice.Api.Services.UnitOfWork;

namespace AccountMicroservice.Api.Services.Hangfire.Jobs
{
    public class RecurringJobs(IUnitOfWork unitOfWork, ILogger<RecurringJobs> logger)
    {
        public async Task RemoveExpiredEmailTokensRecurringJob()
        {
            await unitOfWork.UserEmailTokenRepository.RemoveAllExpiredEmailTokensAsync();
            await unitOfWork.CompleteAsync();
            logger.LogInformation("All expired email tokens has been successfully removed");
        }
    }
}
