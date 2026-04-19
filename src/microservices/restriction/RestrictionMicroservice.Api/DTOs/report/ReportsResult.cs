using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.DTOs.report
{
    public class ReportsResult
    {
        public List<Report> Reports { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
