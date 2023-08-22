namespace WebAPI_tutorial_recursos.DTOs
{
    public class PaginationDTO
    {
        public int Page { get; set; }
        private int recordsPerPage = 10;
        private readonly int pageMaxSize = 50;

        public int RecordsPerPage
        {
            get { return recordsPerPage; }
            set { recordsPerPage = (value > pageMaxSize) ? pageMaxSize : value; }
        }

    }
}
