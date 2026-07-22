namespace ReviewMicroservice.Api.Models
{
    public class Pagination
    {
        private int pageSize;
        public int PageSize
        {
            get => pageSize;
            set
            {
                if (value < 0) pageSize = 0;
                else if (value > 30) pageSize = 30;
                else pageSize = value;
            }
        }

        private int pageNumber;
        public int PageNumber
        {
            get => pageNumber;
            set => pageNumber = value < 0 ? 0 : value;
        }
    }
}
