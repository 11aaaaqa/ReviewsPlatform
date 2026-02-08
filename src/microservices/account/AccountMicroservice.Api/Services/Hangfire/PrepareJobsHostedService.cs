using AccountMicroservice.Api.Services.Hangfire.Jobs;
using Hangfire;

namespace AccountMicroservice.Api.Services.Hangfire
{
    public class PrepareJobsHostedService(IRecurringJobManager recurringJobManager, ILogger<PrepareJobsHostedService> logger) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            recurringJobManager.AddOrUpdate<RecurringJobs>("RemoveExpiredEmailTokens", 
                x => x.RemoveExpiredEmailTokensRecurringJob(), Cron.Daily(9));

            logger.LogInformation("Jobs has been successfully prepared");
            return Task.CompletedTask;
        }
    }
}
