using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.DTOs.restriction
{
    public class RestrictionsResult
    {
        public List<Restriction> Restrictions { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
