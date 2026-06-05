namespace Web.MVC.Models.Api_responses.category
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ReviewsCount { get; set; }
        public List<SubcategoryResponse> Subcategories { get; set; } = new();
    }
}
