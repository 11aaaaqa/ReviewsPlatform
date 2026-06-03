using ReviewMicroservice.Api.Enums;

namespace ReviewMicroservice.Api.Models
{
    public class ReviewNoPictures
    {
        public Guid Id { get; set; }
        public string ShortReview { get; set; }
        public string Text { get; set; }
        public int ItemEstimation { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }

        public Guid ItemId { get; set; }

        public Guid UserId { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsCreatedWithItem { get; set; }
    }
}
