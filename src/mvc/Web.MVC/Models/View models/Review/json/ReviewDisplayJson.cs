namespace Web.MVC.Models.View_models.Review.json
{
    public class ReviewDisplayJson
    {
        public List<ReviewNoPictureDisplay> Reviews { get; set; } = new();
        public bool IsNextPageExisted { get; set; }
    }
}
