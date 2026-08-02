using System.ComponentModel.DataAnnotations;
using ReviewMicroservice.Api.Enums;

namespace ReviewMicroservice.Api.DTOs.review
{
    public class AddRestrictionDto
    {
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
