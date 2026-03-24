using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;

namespace BookovaniMista.Infrastructure
{
    public static class SeedData
    {
        public static void Initialize(BookovaniMistaDbContext db)
        {
            if (!db.Sekce.Any())
            {
                var defaultSekce = new[]
                {
                    new Sekce { Oznaceni = "SJ1", Nazev = "Sekce jih 1", Kapacita = 15 },
                    new Sekce { Oznaceni = "SJ2", Nazev = "Sekce jih 2", Kapacita = 18 }, //asi
                    new Sekce { Oznaceni = "SJ3", Nazev = "Sekce jih 3", Kapacita = 18 }, //asi
                    new Sekce { Oznaceni = "SJ4", Nazev = "Sekce jih 4", Kapacita = 6  }, //asi
                    new Sekce { Oznaceni = "SS1", Nazev = "Sekce sever 1", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS2", Nazev = "Sekce sever 2", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS3", Nazev = "Sekce sever 3", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS4", Nazev = "Sekce sever 4", Kapacita = 18 },
                    new Sekce { Oznaceni = "SS5", Nazev = "Sekce sever 5", Kapacita = 6  },
                };

                db.Sekce.AddRange(defaultSekce);
                db.SaveChanges();
            }

            if (!db.Mista.Any())
            {
                var sekceList = db.Sekce.OrderBy(s => s.Id).ToList();
                var mista = new List<Misto>();

                foreach (var s in sekceList)
                {
                    // nechceme záporné nebo nulové poèty
                    var count = Math.Max(0, s.Kapacita);
                    for (int i = 1; i <= count; i++)
                    {
                        mista.Add(new Misto
                        {
                            Oznaceni = $"{s.Oznaceni}-M{i}",
                            Nazev = $"Místo {i} ({s.Nazev})",
                            Sekce = s
                        });
                    }
                }

                if (mista.Any())
                {
                    db.Mista.AddRange(mista);
                    db.SaveChanges();
                }
            }

            if (!db.Zamestnanci.Any())
            {
                var defaultZamestnanec = new[]
                {
                    new Zamestnanec { Jmeno = "Jan Novák", Email = "jan.novak92@testmail.cz" },
                    new Zamestnanec { Jmeno = "Petra Svobodová", Email = "petra.svobodova87@testmail.cz" },
                    new Zamestnanec { Jmeno = "Martin Dvoøák", Email = "martin.dvorak91@testmail.cz" },
                    new Zamestnanec { Jmeno = "Lucie Procházková", Email = "lucie.prochazkova95@testmail.cz" },
                };

                db.Zamestnanci.AddRange(defaultZamestnanec);
                db.SaveChanges();
            }

            if (!db.Rezervace.Any())
            {
                var mistaList = db.Mista.OrderBy(m => m.Id).ToList();
                var zamestnanciList = db.Zamestnanci.OrderBy(z => z.Id).ToList();

                var defaultRezervace = new[]
                {
                    new Rezervace {  Misto = mistaList[0] , Zamestnanec = zamestnanciList[0] , DatumRezervace = DateTime.Now.AddDays(-1) },
                    new Rezervace {  Misto = mistaList[11] , Zamestnanec = zamestnanciList[1] , DatumRezervace = DateTime.Now.AddDays(-1) },
                    new Rezervace {  Misto = mistaList[22] , Zamestnanec = zamestnanciList[2] , DatumRezervace = DateTime.Now },
                    new Rezervace {  Misto = mistaList[33] , Zamestnanec = zamestnanciList[3] , DatumRezervace = DateTime.Now.AddDays(1) },
                    new Rezervace {  Misto = mistaList[44] , Zamestnanec = zamestnanciList[3] , DatumRezervace = DateTime.Now.AddDays(1) },
                };

                db.Rezervace.AddRange(defaultRezervace);
                db.SaveChanges();
            }
        }
    }
}
