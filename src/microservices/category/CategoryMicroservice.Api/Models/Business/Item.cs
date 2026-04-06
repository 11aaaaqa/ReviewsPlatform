using CategoryMicroservice.Api.Enums;

namespace CategoryMicroservice.Api.Models.Business
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Brand { get; set; }
        public double GeneralEstimation { get; set; }
        public int ReviewsCount { get; set; }
        public byte[] Picture { get; set; }
        public Guid SubcategoryId { get; set; }

        public Guid CreatedByUserId { get; set; }
        public ItemStatus Status { get; set; }
        public string? RejectionReason { get; set; }

        public SagaStatus SagaStatus { get; set; }
    }
}
