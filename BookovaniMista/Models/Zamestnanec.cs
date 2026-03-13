namespace BookovaniMista.Models
{
    public class Zamestnanec
    {
        public int Id { get; set; } 
        public required string Jmeno { get; set; }
        public string? Email { get; set; }
    }
}
