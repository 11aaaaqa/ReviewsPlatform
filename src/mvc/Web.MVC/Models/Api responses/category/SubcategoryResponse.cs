namespace Web.MVC.Models.Api_responses.category
{
    public class SubcategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ReviewsCount { get; set; }
        public Guid CategoryId { get; set; }
    }
}
