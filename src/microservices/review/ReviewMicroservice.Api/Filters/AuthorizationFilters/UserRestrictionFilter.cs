using Microsoft.AspNetCore.Mvc;
using RestrictionGrpcService;

namespace ReviewMicroservice.Api.Filters.AuthorizationFilters
{
    public class UserRestrictionFilter : TypeFilterAttribute
    {
        public UserRestrictionFilter(params RestrictionType[] restrictionTypes) 
            : base(typeof(UserRestrictionFilterService))
        {
            Arguments = new object[] { restrictionTypes };
        }
    }
}
