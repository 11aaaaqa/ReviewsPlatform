namespace Web.MVC.Models.Api_responses.category
{
    public class ItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Brand { get; set; }
        public double GeneralEstimation { get; set; }
        public int ReviewsCount { get; set; }
        public byte[] Picture { get; set; }
        public Guid SubcategoryId { get; set; }

        public ItemStatus Status { get; set; }
    }

    public enum ItemStatus
    {
        Pending,
        UnderConsideration,
        Verified
    }
}
