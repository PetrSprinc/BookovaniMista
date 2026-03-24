namespace Entities.BookovaniMista.Models
{
    public class Misto
    {
        public int Id { get; set; }
        public required string Oznaceni { get; set; }
        public string? Nazev { get; set; }
        public int SekceId { get; set; }
        public required Sekce Sekce { get; set; }

        // Navigační kolekce: jedno místo má mnoho rezervací
        public List<Rezervace> Rezervace { get; set; } = new();
    }
}
