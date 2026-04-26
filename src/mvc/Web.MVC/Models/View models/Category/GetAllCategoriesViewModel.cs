using Web.MVC.Models.Api_responses.category;

namespace Web.MVC.Models.View_models.Category
{
    public class GetAllCategoriesViewModel
    {
        public List<CategoryResponse> Categories { get; set; }
        public bool IsUserAllowedToAddCategory { get; set; }
    }
}
