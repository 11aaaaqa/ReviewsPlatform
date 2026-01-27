using Microsoft.AspNetCore.Mvc;

namespace AccountMicroservice.Api.Filters.ActionFilters
{
    public class ValidatePassedUserIdActionFilter : TypeFilterAttribute
    {
        public ValidatePassedUserIdActionFilter() : base(typeof(ValidatePassedUserIdActionFilterService)) { }
    }
}
