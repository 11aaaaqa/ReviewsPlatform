using CategoryMicroservice.Api.Models.Business;

namespace CategoryMicroservice.Api.DTOs.Items
{
    public class ItemsResult
    {
        public List<Item> Items { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
