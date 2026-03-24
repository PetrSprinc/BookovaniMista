namespace Entities.BookovaniMista
{
    public class ViewModel
    {
        public required List<Models.Sekce> Sekce { get; set; }
        public required List<Models.Misto> Mista { get; set; }
        public required List<Models.Zamestnanec> Zamestnanci { get; set; }
        public required List<Models.Rezervace> Rezervace { get; set; }
    }
}
