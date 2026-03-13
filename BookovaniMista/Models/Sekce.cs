namespace BookovaniMista.Models
{
    public class Sekce
    {
        public int Id { get; set; }
        public required string Nazev { get; set; }
        public int Kapacita { get; set; } = 0;
    }
}
