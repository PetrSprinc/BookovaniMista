using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista.Interfaces;
using Business.BookovaniMista.ViewModels;
using Business.BookovaniMista.Resources;

namespace Business.BookovaniMista
{
    /// <summary>
    /// Běžné operace - obsahuje metody pro identifikaci uživatele a statistiky obsazenosti.
    /// Zajišťuje hledání zaměstnance podle různých kritérií a generuje reporty o využití míst.
    /// </summary>
    public class CommonBusiness : ICommonBusiness
    {
        private readonly BookovaniMistaDbContext _db;
        private readonly IMemoryCache _cache;

        public CommonBusiness(BookovaniMistaDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        /// <summary>
        /// Načítá aktuálního zaměstnance na základě uživatelských claims.
        /// Hledá postupně: email, UPN, jméno z identity.
        /// </summary>
        public async Task<Zamestnanec?> GetCurrentZamestnanecAsync(ClaimsPrincipal user)
        {
            if (user == null) return null;

            var email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value
                        ?? user.FindFirst("upn")?.Value;

            if (!string.IsNullOrEmpty(email))
            {
                var emailNorm = email.Trim().ToLowerInvariant();
                var byEmail = await _db.Zamestnanci
                    .FirstOrDefaultAsync(z => z.Email != null && z.Email.ToLower() == emailNorm);
                if (byEmail != null) return byEmail;
            }

            var nameClaim = user.Identity?.Name;
            if (!string.IsNullOrEmpty(nameClaim))
            {
                var byFull = await _db.Zamestnanci
                    .FirstOrDefaultAsync(z => z.Jmeno != null && z.Jmeno == nameClaim);
                if (byFull != null) return byFull;

                var shortName = nameClaim.Contains('\\') ? nameClaim.Split('\\').Last() : nameClaim;
                if (!string.IsNullOrWhiteSpace(shortName))
                {
                    var escapedShortName = EscapeLikePattern(shortName);
                    var pattern = $"%{escapedShortName}%";
                    var byShort = await _db.Zamestnanci
                        .FirstOrDefaultAsync(z => z.Jmeno != null && EF.Functions.Like(z.Jmeno, pattern, "\\"));
                    if (byShort != null) return byShort;
                }
            }

            return null;
        }

        private static string EscapeLikePattern(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Escape backslash nejprve, aby nedošlo k dvojitému escapování
            return input
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
        }

        /// <summary>
        /// Získává statistiku vytížení míst za období.
        /// Počítá počet dnů, kdy bylo každé místo objednáno.
        /// </summary>
        public async Task<VytizenostResult> GetVytizenostAsync(DateTime? odDatum, DateTime? doDatum)
        {
            var today = DateTime.Today;
            var odDatumCalc = odDatum ?? new DateTime(today.Year, today.Month, 1);
            var doDatumCalc = doDatum ?? odDatumCalc.AddMonths(1).AddDays(-1);

            var data = _db.Rezervace
                .Where(r => r.DatumRezervace >= odDatumCalc && r.DatumRezervace <= doDatumCalc)
                .GroupBy(r => r.MistoId)
                .Select(g => new
                {
                    MistoId = g.Key,
                    DaysCount = g.Select(r => r.DatumRezervace.Date).Distinct().Count()
                });

            var rowsQuery = data
                .Join(_db.Mista.Include(m => m.Sekce),
                      g => g.MistoId,
                      m => m.Id,
                      (g, m) => new VytizenostRow
                      {
                          MistoOznaceni = m.Oznaceni,
                          SekceNazev = string.IsNullOrEmpty(m.Sekce.Nazev) ? m.Sekce.Oznaceni : m.Sekce.Nazev,
                          BookedDays = g.DaysCount
                      })
                .OrderByDescending(r => r.BookedDays);

            var rows = await rowsQuery.ToListAsync();

            return new VytizenostResult
            {
                Od = odDatumCalc,
                Do = doDatumCalc,
                Rows = rows
            };
        }

        /// <summary>
        /// Vrátí centralizované definice sekcí aplikace.
        /// Data se cachují pro 24 hodin.
        /// Používáno v Zabookovat, Obsazenost a dalších views.
        /// </summary>
        /// <returns>Pole sekcí s jejich metadata</returns>
        public SectionInfo[] GetSectionDefinitions()
        {
            const string cacheKey = "SectionDefinitions_All";

            // Pokus načíst z cache
            if (_cache.TryGetValue(cacheKey, out SectionInfo[]? cachedSections) && cachedSections != null)
            {
                return cachedSections;
            }

            // Pokud není v cache, vytvoř data
            var sections = new[]
            {
                // Horní řada: Sekce Jih (SJ)
                new SectionInfo { Id = "SJ1", Db = 1, Title = "Sekce jih 1", Subtitle = "Severní levá sekce — vizualizace míst", Total = 15, Rows = 3 },
                new SectionInfo { Id = "SJ2", Db = 2, Title = "Sekce jih 2", Subtitle = "Severní střed-levo — vizualizace míst", Total = 18, Rows = 3 },
                new SectionInfo { Id = "SJ3", Db = 3, Title = "Sekce jih 3", Subtitle = "Severní střed-pravo — vizualizace míst", Total = 18, Rows = 3 },
                new SectionInfo { Id = "SJ4", Db = 4, Title = "Sekce jih 4", Subtitle = "Severní pravá sekce — vizualizace míst", Total = 6, Rows = 3 },

                // Dolní řada: Sekce Sever (SS)
                new SectionInfo { Id = "SS1", Db = 5, Title = "Sekce sever 1", Subtitle = "Jižní 1/5 — vizualizace míst", Total = 12, Rows = 3 },
                new SectionInfo { Id = "SS2", Db = 6, Title = "Sekce sever 2", Subtitle = "Jižní 2/5 — vizualizace míst", Total = 12, Rows = 3 },
                new SectionInfo { Id = "SS3", Db = 7, Title = "Sekce sever 3", Subtitle = "Jižní 3/5 — vizualizace míst", Total = 12, Rows = 3 },
                new SectionInfo { Id = "SS4", Db = 8, Title = "Sekce sever 4", Subtitle = "Jižní 4/5 — vizualizace míst", Total = 18, Rows = 3 },
                new SectionInfo { Id = "SS5", Db = 9, Title = "Sekce sever 5", Subtitle = "Jižní 5/5 — vizualizace míst", Total = 6, Rows = 3 }
            };

            // Ulož do cache na 24 hodin
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));
            _cache.Set(cacheKey, sections, cacheOptions);

            return sections;
        }

        /// <summary>
        /// Parsuje booking date ze stringu.
        /// Vrátí platný DateTime, nebo DateTime.Today pokud je vstup neplatný.
        /// </summary>
        public static DateTime ParseBookingDate(string? bookingDateString)
        {
            // Výchozí formát
            if (string.IsNullOrWhiteSpace(bookingDateString))
            {
                bookingDateString = DateTime.Today.ToString("yyyy-MM-dd");
            }

            // Pokus se parsovat
            if (DateTime.TryParse(bookingDateString, out DateTime parsedDate))
            {
                return parsedDate.Date;  // Vždy vrať midnight (bez času)
            }

            // Fallback
            return DateTime.Today;
        }
    }
}
