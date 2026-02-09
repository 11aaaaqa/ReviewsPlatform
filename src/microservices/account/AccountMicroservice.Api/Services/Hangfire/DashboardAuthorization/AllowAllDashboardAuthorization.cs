using Hangfire.Dashboard;

namespace AccountMicroservice.Api.Services.Hangfire.DashboardAuthorization
{
    public class AllowAllDashboardAuthorization : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
