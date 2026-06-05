using RestrictionMicroservice.Api.Enums;

namespace RestrictionMicroservice.Api.Models.Business
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid ReportingUserId { get; set; }
        public Guid ReportedUserId { get; set; }
        public Guid ReportOnEntityId { get; set; }
        public ReportType ReportType { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}