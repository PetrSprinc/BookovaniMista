using System.Linq;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;

namespace BookovaniMista.Infrastructure
{
    public static class SeedData
    {
        // Pøijímá DbContext a provede seed
        public static void Initialize(BookovaniMistaDbContext db)
        {
            // Pro InMemory provider EnsureCreated není vždy nutné, ale nevadí
            db.Database.EnsureCreated();

            if (!db.Sekce.Any())
            {
                var defaultSekce = new[]
                {
                    new Sekce { Nazev = "Sekce 1", Kapacita = 10 },
                    new Sekce { Nazev = "Sekce 2", Kapacita = 12 },
                    new Sekce { Nazev = "Sekce 3", Kapacita = 8  },
                    new Sekce { Nazev = "Sekce 4", Kapacita = 15 },
                    new Sekce { Nazev = "Sekce 5", Kapacita = 20 },
                    new Sekce { Nazev = "Sekce 6", Kapacita = 6  },
                    new Sekce { Nazev = "Sekce 7", Kapacita = 18 },
                    new Sekce { Nazev = "Sekce 8", Kapacita = 14 },
                    new Sekce { Nazev = "Sekce 9", Kapacita = 9  },
                };

                db.Sekce.AddRange(defaultSekce);
                db.SaveChanges();
            }
        }
    }
}
