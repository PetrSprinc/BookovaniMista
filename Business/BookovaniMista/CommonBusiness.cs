using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista.Interfaces;
using Business.BookovaniMista.ViewModels;

namespace Business.BookovaniMista
{
    public class CommonBusiness : ICommonBusiness
    {
        private readonly BookovaniMistaDbContext _db;

        public CommonBusiness(BookovaniMistaDbContext db) => _db = db;

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

            // Escape backslash first to avoid double-escaping
            return input
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
        }

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
    }
}
