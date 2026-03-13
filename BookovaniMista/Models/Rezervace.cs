namespace BookovaniMista.Models
{
    public class Rezervace
    {
        public int Id { get; set; }
        public int MistoId { get; set; }
        public required Misto Misto { get; set; }
        public int ZamestnanecId { get; set; }
        public required Zamestnanec Zamestnanec { get; set; }
        public required DateTime DatumRezervace { get; set; }
    }
}
