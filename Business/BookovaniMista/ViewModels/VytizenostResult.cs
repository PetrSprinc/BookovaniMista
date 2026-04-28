namespace Business.BookovaniMista.ViewModels
{
    public class VytizenostResult
    {
        public DateTime Od { get; set; }
        public DateTime Do { get; set; }
        public List<VytizenostRow> Rows { get; set; } = new();
    }
}
