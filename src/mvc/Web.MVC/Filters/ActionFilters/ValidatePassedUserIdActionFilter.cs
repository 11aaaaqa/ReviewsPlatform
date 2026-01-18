using Microsoft.AspNetCore.Mvc;

namespace Web.MVC.Filters.ActionFilters
{
    public class ValidatePassedUserIdActionFilter : TypeFilterAttribute
    {
        public ValidatePassedUserIdActionFilter() : base(typeof(ValidatePassedUserIdActionFilterService)) { }
    }
}
