namespace Entities.BookovaniMista.Models
{
    public class Zamestnanec
    {
        public int Id { get; set; }
        public required string Jmeno { get; set; }
        public required string Email { get; set; }

        // Navigační kolekce: jeden zaměstnanec má mnoho rezervací
        public List<Rezervace> Rezervace { get; set; } = new();
    }
}
