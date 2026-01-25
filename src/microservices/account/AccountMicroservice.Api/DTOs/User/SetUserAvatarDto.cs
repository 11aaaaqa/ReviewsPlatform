using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.User
{
    public class SetUserAvatarDto
    {
        public Guid UserId { get; set; }
        [Required]
        public byte[] AvatarSource { get; set; }
    }
}
