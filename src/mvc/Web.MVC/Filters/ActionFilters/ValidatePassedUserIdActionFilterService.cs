using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Web.MVC.Filters.ActionFilters
{
    public class ValidatePassedUserIdActionFilterService(ILogger logger) : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            object? userIdObj = context.ActionArguments["userId"];
            if (userIdObj == null)
            {
                context.Result = new BadRequestResult();
                return;
            }

            string userIdStr = userIdObj.ToString()!;
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                context.Result = new BadRequestResult();
                return;
            }

            string actualUserIdStr = context.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (!Guid.TryParse(actualUserIdStr, out Guid actualUserId))
            {
                logger.LogCritical("User's {UserId} access token do not have NameIdentifier claim with Id as a Guid type", actualUserIdStr);
                context.Result = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                return;
            }

            if (userId != actualUserId)
            {
                logger.LogWarning("User {UserId} tried to access to an Editing user profile page with not his Id", actualUserId);
                context.Result = new ForbidResult();
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
