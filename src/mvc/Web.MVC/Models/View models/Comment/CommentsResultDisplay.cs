namespace Web.MVC.Models.View_models.Comment
{
    public class CommentsResultDisplay
    {
        public List<CommentDisplay> Comments { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
