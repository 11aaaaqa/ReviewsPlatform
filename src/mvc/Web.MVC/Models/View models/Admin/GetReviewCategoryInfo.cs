namespace Web.MVC.Models.View_models.Admin
{
    public record GetReviewCategoryInfo(
        string CategoryName,
        Guid SubcategoryId,
        string SubcategoryName);
}
