namespace ReviewMicroservice.Api.Models
{
    public class Pagination
    {
        private int pageSize;
        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > 30 ? 30 : value;
        }

        private int pageNumber;
        public int PageNumber
        {
            get => pageNumber;
            set => pageNumber = value < 0 ? 0 : value;
        }
    }
}
