using RestrictionMicroservice.Api.Enums;

namespace RestrictionMicroservice.Api.Models.Business
{
    public class Restriction
    {
        public Guid Id { get; set; }
        public Guid RestrictedUserId { get; set; }
        public Guid RestrictingUserId { get; set; }
        public RestrictionType RestrictionType { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsPermanent { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
