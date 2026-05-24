using Web.MVC.Models.Api_responses.category;

namespace Web.MVC.Services
{
    public class SortService
    {
        public List<CategoryResponse> SortCategories(List<CategoryResponse> categories)
        {
            categories = categories
                .OrderBy(x => x.Name.ToLower().Contains("разное") || x.Name.ToLower().Contains("другое"))
                .ThenBy(x => x.ReviewsCount)
                .Select(x => new CategoryResponse
                {
                    Name = x.Name,
                    Id = x.Id,
                    ReviewsCount = x.ReviewsCount,
                    Subcategories = x.Subcategories
                        .OrderBy(c => c.Name.ToLower().Contains("разное") || c.Name.ToLower().Contains("другое"))
                        .ThenBy(c => c.ReviewsCount).ToList()
                }).ToList();
            return categories;
        }
    }
}
