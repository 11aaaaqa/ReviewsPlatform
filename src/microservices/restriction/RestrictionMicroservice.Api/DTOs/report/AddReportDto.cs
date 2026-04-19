using System.ComponentModel.DataAnnotations;
using RestrictionMicroservice.Api.Enums;

namespace RestrictionMicroservice.Api.DTOs.report
{
    public class AddReportDto
    {
        [Required]
        public Guid ReportedUserId { get; set; }

        [Required]
        public Guid ReportOnEntityId { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
