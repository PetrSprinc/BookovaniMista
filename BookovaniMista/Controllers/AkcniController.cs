using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;

namespace BookovaniMista.Controllers
{
    [Route("Akcni")]
    public class AkcniController : Controller
    {
        private readonly ILogger<AkcniController> _logger;
        private readonly BookovaniMistaDbContext _db;

        public AkcniController(ILogger<AkcniController> logger, BookovaniMistaDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Zabookovat()
        {
            return View();
        }

        // DTO odpovídá payloadu z klienta
        public class RezervaceDto
        {
            public int? sekceId { get; set; }
            public string? seatNumber { get; set; }
            public string? date { get; set; }
        }

        // POST /Akcni/Rezervovat
        [HttpPost("Rezervovat")]
        [IgnoreAntiforgeryToken] // TODO
        public async Task<IActionResult> Rezervovat([FromBody] RezervaceDto dto)
        {
            if (dto == null || dto.sekceId == null)
                return BadRequest("Chybí data rezervace (sekceId).");

            var sekceId = dto.sekceId.Value;

            // najít sekci
            var sekce = await _db.Sekce.FirstOrDefaultAsync(s => s.Id == sekceId);
            if (sekce == null)
                return BadRequest("Sekce nenalezena.");

            // parsování èísla místa (pokud bylo pøedáno)
            int seatIndex = 0;
            if (!string.IsNullOrEmpty(dto.seatNumber))
                int.TryParse(dto.seatNumber, out seatIndex);

            Misto? misto = null;

            // preferované hledání podle oznaèení (SJ1-M1)
            if (seatIndex > 0 && !string.IsNullOrEmpty(sekce.Oznaceni))
            {
                var expected = $"{sekce.Oznaceni}-M{seatIndex}";
                misto = await _db.Mista
                    .Include(m => m.Sekce)
                    .FirstOrDefaultAsync(m => m.Oznaceni == expected && m.Sekce != null && m.Sekce.Id == sekceId);
            }

            // fallback: i-th místo v sekci (øazeno podle Id)
            if (misto == null && seatIndex > 0)
            {
                misto = await _db.Mista
                    .Include(m => m.Sekce)
                    .Where(m => m.Sekce != null && m.Sekce.Id == sekceId)
                    .OrderBy(m => m.Id)
                    .Skip(Math.Max(0, seatIndex - 1))
                    .FirstOrDefaultAsync();
            }

            if (misto == null)
                return BadRequest("Místo nenalezeno pro zadanou sekci/èíslo.");

            // použít aktuálnì pøihlášeného uživatele, musí být v databázi Zamestnanec
            // TODO IsAuthenticated
            // TODO zakomponovat email z claimù
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return Unauthorized();
            string? userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value
                                     ?? User.FindFirst("email")?.Value
                                     ?? User.Identity?.Name;
            Zamestnanec? zam = null;
            if (!string.IsNullOrEmpty(userIdentifier))
            {
                // nejprve podle emailu, pak podle jména
                zam = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email != null && z.Email == userIdentifier);
                if (zam == null)
                    zam = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Jmeno == userIdentifier);
            }
            if (zam == null)
                return BadRequest("Pøihlášený uživatel nenalezen v DB.");

            //OK parsování data rezervace 
            DateTime datumRezervace;
            if (string.IsNullOrEmpty(dto.date) || !DateTime.TryParse(dto.date, out datumRezervace))
                datumRezervace = DateTime.Today;
            else
                datumRezervace = datumRezervace.Date;

            //OK kontrola, zda místo není již zarezervované pro zvolený den
            var alreadyBooked = await _db.Rezervace.IsMistoBookedAsync(misto.Id, datumRezervace);
            if (alreadyBooked)
                return BadRequest("Toto místo je již zarezervované pro zvolený den.");

            //OK vytvoøit rezervaci
            var rezervace = new Rezervace
            {
                MistoId = misto.Id,
                Misto = misto,
                ZamestnanecId = zam.Id,
                Zamestnanec = zam,
                DatumRezervace = datumRezervace
            };

            _db.Rezervace.Add(rezervace);
            await _db.SaveChangesAsync();

            // vrátit úspìch a informace o rezervaci
            return Ok(new
            {
                success = true,
                rezervaceId = rezervace.Id,
                mistoId = misto.Id,
                sekceId = sekce.Id,
                date = datumRezervace.ToString("yyyy-MM-dd")
            });
        }
    }
}