using AccountMicroservice.Api.Enums.SortEnums;
using AccountMicroservice.Api.Models;

namespace AccountMicroservice.Api.DTOs.User
{
    public class GetUsersDto
    {
        public Pagination Pagination { get; set; }
        public string? SearchQuery { get; set; } = null;
        public List<Guid>? RoleIds { get; set; } = null;
        public UserSort UserSort { get; set; } = UserSort.None;
    }
}
