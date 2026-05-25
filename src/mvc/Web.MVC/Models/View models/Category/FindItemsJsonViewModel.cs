namespace Web.MVC.Models.View_models.Category
{
    public class FindItemsJsonViewModel
    {
        public List<ItemDisplay> Items { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
