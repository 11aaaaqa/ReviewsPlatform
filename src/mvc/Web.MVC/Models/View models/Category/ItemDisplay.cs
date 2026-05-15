namespace Web.MVC.Models.View_models.Category
{
    public class ItemDisplay
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Brand { get; set; }
        public double GeneralEstimation { get; set; }
        public int ReviewsCount { get; set; }
        public string PictureSrc { get; set; }
        public Guid SubcategoryId { get; set; }
    }
}
