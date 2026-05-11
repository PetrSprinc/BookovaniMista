namespace Business.BookovaniMista.ViewModels
{
    public class SectionInfo
    {
        public required string Id { get; set; }
        public int Db { get; set; }
        public required string Title { get; set; }
        public required string Subtitle { get; set; }
        public int Total { get; set; }
        public int Rows { get; set; }
    }
}
