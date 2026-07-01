using Web.MVC.Models.View_models.Category;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Models.View_models.Review
{
    public class ReviewDisplay
    {
        public Guid Id { get; set; }
        public string ShortReview { get; set; }
        public string Text { get; set; }
        public int ItemEstimation { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int CommentsCount { get; set; }
        public List<string> PicturesSrc { get; set; } = new();

        public ItemDisplay Item { get; set; }
        public UserDisplay CreatedByUser { get; set; }
    }
}
