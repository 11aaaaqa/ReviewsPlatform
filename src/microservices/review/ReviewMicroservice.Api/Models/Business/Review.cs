using ReviewMicroservice.Api.Enums;

namespace ReviewMicroservice.Api.Models.Business
{
    public class Review
    {
        public Guid Id { get; set; }
        public string ShortReview { get; set; }
        public string Text { get; set; }
        public int ItemEstimation { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public List<byte[]> Pictures { get; set; } = new();

        public Guid ItemId { get; set; }

        public Guid UserId { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
        public string? RejectionReason { get; set; }
        public bool CreatedWithItem { get; set; }
    }
}
