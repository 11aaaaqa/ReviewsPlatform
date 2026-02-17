namespace CategoryMicroservice.Api.Models.Business
{
    public class Subcategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ReviewsCount { get; set; } = 0;
        public Guid CategoryId { get; set; }
    }
}
