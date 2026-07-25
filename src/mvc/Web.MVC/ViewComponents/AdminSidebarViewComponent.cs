using Microsoft.AspNetCore.Mvc;
using Web.MVC.Models;

namespace Web.MVC.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            const string controllerName = "Admin";

            const string GetUsersList = nameof(GetUsersList);
            const string GetReviews = nameof(GetReviews);
            const string GetReviewById = nameof(GetReviewById);

            var items = new List<AdminMenuItem>
            {
                new() { Controller = controllerName, Action = GetUsersList, Title = "Пользователи" },
                new() { Controller = controllerName, Action = GetReviews, Title = "Отзывы" }
            };

            string currentController = ViewContext.RouteData.Values["controller"]!.ToString()!;
            string currentAction = ViewContext.RouteData.Values["action"]!.ToString()!;

            var selectedItem = items.SingleOrDefault(x => x.Action == currentAction && x.Controller == currentController);
            if (selectedItem != null)
            {
                selectedItem.IsActive = true;
            }
            else
            {
                if (currentController == controllerName && currentAction == GetReviewById)
                {
                    var getReviewsItem = items.SingleOrDefault(x => x.Controller == controllerName && x.Action == GetReviews);
                    getReviewsItem?.IsActive = true;
                }
            }

            return View(items);
        }
    }
}
