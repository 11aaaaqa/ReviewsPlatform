using System.ComponentModel.DataAnnotations;
using RestrictionMicroservice.Api.Enums;

namespace RestrictionMicroservice.Api.DTOs.restriction
{
    public class AddRestrictionDto
    {
        [Required]
        public Guid RestrictedUserId { get; set; }

        [Required]
        public RestrictionType RestrictionType { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Required]
        public bool IsPermanent { get; set; }

        [Required]
        [StringLength(250)]
        public string Reason { get; set; }
    }
}
