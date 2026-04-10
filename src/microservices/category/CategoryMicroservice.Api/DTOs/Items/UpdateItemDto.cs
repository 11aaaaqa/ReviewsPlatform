using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.Api.DTOs.Items
{
    public class UpdateItemDto
    {
        [Required]
        public Guid Id { get; set; }

        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [RegularExpression("^[A-ZА-ЯЁ0-9]")]
        [StringLength(100)]
        public string? Brand { get; set; }
    }
}
