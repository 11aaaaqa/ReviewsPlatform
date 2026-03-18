namespace CategoryMicroservice.Api.Models.Business
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ReviewsCount { get; set; } = 0;
        public List<Subcategory> Subcategories { get; set; }
    }
}
