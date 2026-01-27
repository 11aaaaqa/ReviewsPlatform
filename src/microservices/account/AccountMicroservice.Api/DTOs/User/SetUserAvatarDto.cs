using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.User
{
    public class SetUserAvatarDto
    {
        [Required]
        public byte[] AvatarSource { get; set; }
    }
}
