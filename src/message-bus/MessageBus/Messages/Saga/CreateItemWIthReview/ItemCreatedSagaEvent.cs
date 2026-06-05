using MessageBus.Abstractions;

namespace MessageBus.Messages.Saga.CreateItemWIthReview
{
    public class ItemCreatedSagaEvent : MessageBase
    {
        public Guid ItemId { get; set; }
        public string ShortReview { get; set; }

        public string ReviewText { get; set; }

        public int ReviewItemEstimation { get; set; }

        public List<byte[]> ReviewPictures { get; set; } = new();
        public Guid UserIdCreatedBy { get; set; }
    }
}
