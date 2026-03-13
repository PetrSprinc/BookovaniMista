namespace BookovaniMista.Models
{
    public class Misto
    {
        public int Id { get; set; }
        public required string Nazev { get; set; }
        public int SekceId { get; set; }
        public required Sekce Sekce { get; set; }
    }
}
