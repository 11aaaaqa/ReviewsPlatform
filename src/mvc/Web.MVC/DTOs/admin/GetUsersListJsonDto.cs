using Web.MVC.Models.Api_responses.account.enums;

namespace Web.MVC.DTOs.admin
{
    public class GetUsersListJsonDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string? SearchQuery { get; set; }
        public UserSort UserSort { get; set; } = UserSort.None;
        public List<Guid>? RoleIds { get; set; } = null;
    }
}
