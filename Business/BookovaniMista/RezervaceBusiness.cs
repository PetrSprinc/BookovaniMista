using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista.Interfaces;

namespace Business.BookovaniMista
{
    public class RezervaceBusiness : IRezervaceBusiness
    {
        private readonly BookovaniMistaDbContext _db;

        public RezervaceBusiness(BookovaniMistaDbContext db) => _db = db;

        // Hlavní orchestrace rezervace — tenká vrstva, deleguje jednotlivé kroky do privátních metod
        public async Task<(bool Success, string? Error, Rezervace? Rezervace)> RezervovatAsync(RezervaceDto dto, string userIdentifier, CancellationToken cancellationToken = default)
        {
            // validace DTO
            var dtoValidation = ValidateDto(dto);
            if (!dtoValidation.IsValid)
                return (false, dtoValidation.Error, null);

            var sekceId = dto!.SekceId!.Value;

            // naèíst sekci
            var sekce = await GetSekceAsync(sekceId, cancellationToken);
            if (sekce == null)
                return (false, "Sekce nenalezena.", null);

            // parsování èísla místa
            var seatIndex = ParseSeatIndex(dto);

            // najít místo
            var misto = await FindMistoAsync(sekceId, seatIndex, sekce, cancellationToken);
            if (misto == null)
                return (false, "Místo nenalezeno pro zadanou sekci/èíslo.", null);

            // najít zamìstnance podle userIdentifier
            var zam = await FindZamestnanecAsync(userIdentifier, cancellationToken);
            if (zam == null)
                return (false, "Pøihlášený uživatel nenalezen v DB.", null);

            // parsování data rezervace
            var datumRezervace = ParseDatumRezervace(dto.Date);

            // validace data rezervace (vèetnì rozsahu)
            var datumValidation = ValidateDatumRezervace(datumRezervace);
            if (!datumValidation.IsValid)
                return (false, datumValidation.Error, null);

            // kontrola kolize
            var booked = await IsMistoBookedAsync(misto.Id, datumRezervace, cancellationToken);
            if (booked)
                return (false, "Místo již zarezervované pro zvolený den.", null);

            // vytvoøení rezervace a uložení
            var saved = await CreateRezervaceAsync(misto.Id, zam.Id, datumRezervace, cancellationToken);
            if (saved == null)
                return (false, "Chyba pøi ukládání rezervace (možná kolize).", null);

            return (true, null, saved);
        }

        // ---------- Privátní pomocné metody ----------
        private Task<bool> IsMistoBookedAsync(int mistoId, DateTime date, CancellationToken ct)
        {
            var d = date.Date;
            return _db.Rezervace.AnyAsync(r => r.MistoId == mistoId && r.DatumRezervace >= d && r.DatumRezervace < d.AddDays(1), ct);
        }
        private (bool IsValid, string? Error) ValidateDto(RezervaceDto? dto)
        {
            if (dto == null) return (false, "Chybí data rezervace.");
            if (dto.SekceId == null) return (false, "Chybí sekce (SekceId).");
            if (string.IsNullOrEmpty(dto.SeatNumber)) return (false, "Chybí èíslo místa (SeatNumber).");
            if (string.IsNullOrEmpty(dto.Date)) return (false, "Chybí datum rezervace (Date).");
            return (true, null);
        }

        private Task<Sekce?> GetSekceAsync(int sekceId, CancellationToken ct)
            => _db.Sekce.FirstOrDefaultAsync(s => s.Id == sekceId, ct);

        private int ParseSeatIndex(RezervaceDto dto)
        {
            if (string.IsNullOrEmpty(dto.SeatNumber)) return 0;
            return int.TryParse(dto.SeatNumber, out var idx) ? idx : 0;
        }

        private async Task<Misto?> FindMistoAsync(int sekceId, int seatIndex, Sekce? sekce, CancellationToken ct)
        {
            if (seatIndex <= 0) return null;

            // preferované hledání podle oznaèení (SJ1-M1)
            if (sekce != null && !string.IsNullOrEmpty(sekce.Oznaceni))
            {
                var expected = $"{sekce.Oznaceni}-M{seatIndex}";
                var byOzn = await _db.Mista
                    .Include(m => m.Sekce)
                    .FirstOrDefaultAsync(m => m.Oznaceni == expected && m.SekceId == sekceId, ct);
                if (byOzn != null) return byOzn;
            }

            // fallback: i-th místo v sekci (øazeno podle Id)
            return await _db.Mista
                .Include(m => m.Sekce)
                .Where(m => m.SekceId == sekceId)
                .OrderBy(m => m.Id)
                .Skip(Math.Max(0, seatIndex - 1))
                .FirstOrDefaultAsync(ct);
        }

        private async Task<Zamestnanec?> FindZamestnanecAsync(string? userIdentifier, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(userIdentifier)) return null;

            // nejprve podle emailu, pak podle jména
            var byEmail = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email != null && z.Email == userIdentifier, ct);
            if (byEmail != null) return byEmail;

            return await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Jmeno == userIdentifier, ct);
        }

        private DateTime ParseDatumRezervace(string? date)
        {
            if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, out var d))
                return DateTime.Today;
            return d.Date;
        }
        private (bool IsValid, string? Error) ValidateDatumRezervace(DateTime datum)
        {
            const int maxDaysInFuture = 365;

            // Check if date is in the past
            if (datum < DateTime.Today)
                return (false, "Nelze zarezervovat místo v minulosti.");

            // Check if date is too far in the future
            if (datum > DateTime.Today.AddDays(maxDaysInFuture))
                return (false, $"Nelze zarezervovat místo více než {maxDaysInFuture} dní dopøedu.");

            return (true, null);
        }
        private async Task<Rezervace?> CreateRezervaceAsync(int mistoId, int zamestnanecId, DateTime datum, CancellationToken ct)
        {
            var misto = await _db.Mista.Include(m => m.Sekce).FirstOrDefaultAsync(m => m.Id == mistoId, ct);
            var zamestnanec = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Id == zamestnanecId, ct);
            if (misto == null || zamestnanec == null)
                return null;

            var rezervace = new Rezervace
            {
                MistoId = mistoId,
                Misto = misto,
                ZamestnanecId = zamestnanecId,
                Zamestnanec = zamestnanec,
                DatumRezervace = datum.Date
            };

            _db.Rezervace.Add(rezervace);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // pravdìpodobná kolize / porušení DB constraintu
                return null;
            }

            return await _db.Rezervace
                .Include(r => r.Misto).ThenInclude(m => m.Sekce)
                .Include(r => r.Zamestnanec)
                .FirstOrDefaultAsync(r => r.Id == rezervace.Id, ct);
        }
    }
}