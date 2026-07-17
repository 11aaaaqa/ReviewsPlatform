using Microsoft.AspNetCore.Mvc.Filters;
using RestrictionGrpcService;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ReviewMicroservice.Api.Filters.AuthorizationFilters
{
    public class UserRestrictionFilterService : Attribute, IAsyncAuthorizationFilter
    {
        private readonly RestrictionInfo.RestrictionInfoClient restrictionInfoClient;
        private readonly ILogger<UserRestrictionFilterService> logger;
        private RestrictionType[] restrictionTypes;

        public UserRestrictionFilterService(RestrictionInfo.RestrictionInfoClient restrictionInfoClient,
            ILogger<UserRestrictionFilterService> logger, params RestrictionType[] restrictionTypes)
        {
            this.restrictionInfoClient = restrictionInfoClient;
            this.restrictionTypes = restrictionTypes;
            this.logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string userIdStr = context.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var restrictionInfoReply = await restrictionInfoClient.GetRestrictionInfoAsync(
                    new GetRestrictionInfoRequest { UserId = userIdStr });

                if (restrictionInfoReply.RestrictionType != RestrictionType.NoRestrictions)
                {
                    foreach (var restrictionType in restrictionTypes)
                    {
                        if (restrictionInfoReply.RestrictionType == restrictionType)
                            context.Result = new ForbidResult();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Rpc call threw an exception while trying to reach Restriction microservice");
                context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
