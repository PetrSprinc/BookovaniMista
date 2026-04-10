using Entities.BookovaniMista.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.BookovaniMista
{
    public static class SeedData
    {
        public static async Task InitializeAsync(BookovaniMistaDbContext db)
        {
            if (!await db.Sekce.AnyAsync())
            {
                var defaultSekce = new[] //duplicitní s Zabookovat
                {
                    new Sekce { Oznaceni = "SJ1", Nazev = "Sekce jih 1", Kapacita = 15 },
                    new Sekce { Oznaceni = "SJ2", Nazev = "Sekce jih 2", Kapacita = 18 },
                    new Sekce { Oznaceni = "SJ3", Nazev = "Sekce jih 3", Kapacita = 18 },
                    new Sekce { Oznaceni = "SJ4", Nazev = "Sekce jih 4", Kapacita = 6  },
                    new Sekce { Oznaceni = "SS1", Nazev = "Sekce sever 1", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS2", Nazev = "Sekce sever 2", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS3", Nazev = "Sekce sever 3", Kapacita = 12 },
                    new Sekce { Oznaceni = "SS4", Nazev = "Sekce sever 4", Kapacita = 18 },
                    new Sekce { Oznaceni = "SS5", Nazev = "Sekce sever 5", Kapacita = 6  },
                };

                await db.Sekce.AddRangeAsync(defaultSekce);
                await db.SaveChangesAsync();
            }

            if (!await db.Mista.AnyAsync())
            {
                var sekceList = await db.Sekce.OrderBy(s => s.Id).ToListAsync();
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
                    await db.Mista.AddRangeAsync(mista);
                    await db.SaveChangesAsync();
                }
            }

            if (!await db.Zamestnanci.AnyAsync())
            {
                var defaultZamestnanec = new[]
                {
                    new Zamestnanec { Jmeno = "Jan Novák", Email = "jan.novak92@testmail.cz" },
                    new Zamestnanec { Jmeno = "SEYFOR\\petr.sprinc", Email = "Petr.Sprinc@seyfor.com" },
                    new Zamestnanec { Jmeno = "Petra Svobodová", Email = "petra.svobodova87@testmail.cz" },
                    new Zamestnanec { Jmeno = "Martin Dvoøák", Email = "martin.dvorak91@testmail.cz" },
                    new Zamestnanec { Jmeno = "Lucie Procházková", Email = "lucie.prochazkova95@testmail.cz" },
                };

                await db.Zamestnanci.AddRangeAsync(defaultZamestnanec);
                await db.SaveChangesAsync();
            }

            if (!await db.Rezervace.AnyAsync())
            {
                var mistaList = await db.Mista.OrderBy(m => m.Id).ToListAsync();
                var zamestnanciList = await db.Zamestnanci.OrderBy(z => z.Id).ToListAsync();

                var defaultRezervace = new[]
                {
                    new Rezervace {  Misto = mistaList[0] , Zamestnanec = zamestnanciList[0] , DatumRezervace = DateTime.Now.AddDays(-1).Date },
                    new Rezervace {  Misto = mistaList[11] , Zamestnanec = zamestnanciList[1] , DatumRezervace = DateTime.Now.AddDays(-1).Date },
                    new Rezervace {  Misto = mistaList[22] , Zamestnanec = zamestnanciList[2] , DatumRezervace = DateTime.Now.Date },
                    new Rezervace {  Misto = mistaList[33] , Zamestnanec = zamestnanciList[3] , DatumRezervace = DateTime.Now.AddDays(1).Date },
                    new Rezervace {  Misto = mistaList[44] , Zamestnanec = zamestnanciList[3] , DatumRezervace = DateTime.Now.AddDays(1).Date },
                    new Rezervace {  Misto = mistaList[44] , Zamestnanec = zamestnanciList[0] , DatumRezervace = DateTime.Now.AddDays(2).Date },
                    new Rezervace {  Misto = mistaList[44] , Zamestnanec = zamestnanciList[0] , DatumRezervace = DateTime.Now.AddDays(3).Date },
                };

                await db.Rezervace.AddRangeAsync(defaultRezervace);
                await db.SaveChangesAsync();
            }
        }
    }
}
