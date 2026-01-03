namespace AccountMicroservice.Api.DTOs.User
{
    public class SetUserRolesDto
    {
        public Guid UserId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}
