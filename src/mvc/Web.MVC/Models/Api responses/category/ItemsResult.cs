namespace Web.MVC.Models.Api_responses.category
{
    public class ItemsResult
    {
        public List<ItemResponse> Items { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
