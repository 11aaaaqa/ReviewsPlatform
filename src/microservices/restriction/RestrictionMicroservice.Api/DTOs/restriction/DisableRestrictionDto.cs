using System.ComponentModel.DataAnnotations;

namespace RestrictionMicroservice.Api.DTOs.restriction
{
    public class DisableRestrictionDto
    {
        [Required]
        public Guid RestrictionId { get; set; }

        [Required]
        [StringLength(300)]
        public string DisablingReason { get; set; }
    }
}
