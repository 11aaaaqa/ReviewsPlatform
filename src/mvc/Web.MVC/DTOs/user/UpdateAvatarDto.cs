using System.ComponentModel.DataAnnotations;

namespace Web.MVC.DTOs.user
{
    public class UpdateAvatarDto
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
