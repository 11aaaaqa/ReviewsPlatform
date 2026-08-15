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
            const string GetComments = nameof(GetComments);
            const string GetCommentById = nameof(GetCommentById);

            var items = new List<AdminMenuItem>
            {
                new() { Controller = controllerName, Action = GetUsersList, Title = "Пользователи" },
                new() { Controller = controllerName, Action = GetReviews, Title = "Отзывы" },
                new() { Controller = controllerName, Action = GetComments, Title = "Комментарии" }
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
                if (currentController == controllerName)
                {
                    switch (currentAction)
                    {
                        case GetReviewById:
                        {
                            var getReviewsItem = items.SingleOrDefault(x => x.Controller == controllerName && x.Action == GetReviews);
                            getReviewsItem?.IsActive = true;
                            break;
                        }
                        case GetCommentById:
                        {
                            var getCommentsItem = items.SingleOrDefault(x => x.Controller == controllerName && x.Action == GetComments);
                            getCommentsItem?.IsActive = true;
                            break;
                        }
                    }
                }
            }

            return View(items);
        }
    }
}
