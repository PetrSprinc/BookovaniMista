namespace Entities.BookovaniMista.Models
{
    public class Sekce
    {
        public int Id { get; set; }
        public required string Oznaceni { get; set; }
        public string? Nazev { get; set; }
        public int Kapacita { get; set; }

        // Navigační kolekce: jedna sekce má mnoho míst
        public List<Misto> Mista { get; set; } = new();
    }
}
