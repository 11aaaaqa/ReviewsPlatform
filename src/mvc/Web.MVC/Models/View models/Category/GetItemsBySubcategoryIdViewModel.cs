using Web.MVC.Models.Api_responses.category;

namespace Web.MVC.Models.View_models.Category
{
    public class GetItemsBySubcategoryIdViewModel
    {
        public SubcategoryResponse Subcategory { get; set; }
        public List<ItemDisplay> Items { get; set; }
        public bool IsNextPageExisted { get; set; }
        public int PageSize { get; set; }
    }
}
