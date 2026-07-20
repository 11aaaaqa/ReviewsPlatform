using Microsoft.AspNetCore.Mvc;
using Web.MVC.Models;

namespace Web.MVC.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            const string controllerName = "Admin";
            var items = new List<AdminMenuItem>
            {
                new() { Controller = controllerName, Action = "GetUsersList", Title = "Пользователи" }
            };

            string currentController = ViewContext.RouteData.Values["controller"]!.ToString()!;
            string currentAction = ViewContext.RouteData.Values["action"]!.ToString()!;

            var selectedItem = items.SingleOrDefault(x => x.Action == currentAction && x.Controller == currentController);
            selectedItem?.IsActive = true;

            return View(items);
        }
    }
}
