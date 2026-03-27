using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BookovaniMista.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookovaniMista.Controllers
{
    public class ZobrazeniController : Controller
    {
        private readonly ILogger<ZobrazeniController> _logger;
        private readonly BookovaniMistaDbContext _context;

        public ZobrazeniController(ILogger<ZobrazeniController> logger, BookovaniMistaDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Historie: zobrazí pouze rezervace vytvoøené pøihlášeným uživatelem (max 100 posledních)
        public async Task<IActionResult> Historie()
        {
            // Najít aktuálního zamìstnance podle claimu (email / upn / User.Identity.Name)
            var zam = await GetCurrentZamestnanecAsync();
            if (zam == null)
            {
                ViewData["ErrorMessage"] = "Nepodaøilo se identifikovat uživatele v DB (zkontrolujte email).";
                return View(new List<Rezervace>());
            }

            // Naèíst posledních 100 rezervací tohoto zamìstnance (s místem a sekcí)
            var rezervace = await _context.Rezervace
                .Where(r => r.ZamestnanecId == zam.Id)
                .Include(r => r.Misto)
                    .ThenInclude(m => m.Sekce)
                .OrderByDescending(r => r.DatumRezervace)
                .Take(100)
                .ToListAsync();

            return View(rezervace);
        }

        public IActionResult Obsazenost()
        {
            return View();
        }

        // Vytíženost: volitelnì od/do (GET), výchozí od zaèátku mìsíce do jeho konce
        public IActionResult Vytizenost(DateTime? odDatum, DateTime? doDatum)
        {
            var today = DateTime.Today;
            var odDatumCalc = odDatum ?? new DateTime(today.Year, today.Month, 1);
            var doDatumCalc = doDatum ?? odDatumCalc.AddMonths(1).AddDays(-1);

            // Vyber rezervací v intervalu, spoèítat DISTINCT datumy (poèet dnù) pro každé místo
            var data = _context.Rezervace
                .Where(r => r.DatumRezervace >= odDatumCalc && r.DatumRezervace <= doDatumCalc)
                .GroupBy(r => r.MistoId)
                .Select(g => new
                {
                    MistoId = g.Key,
                    DaysCount = g.Select(r => r.DatumRezervace.Date).Distinct().Count()
                });

            // Pøipojit informace o místì a sekci
            var rows = data
                .Join(_context.Mista.Include(m => m.Sekce),
                      g => g.MistoId,
                      m => m.Id,
                      (g, m) => new VytizenostRadek
                      {
                          MistoOznaceni = m.Oznaceni,
                          SekceNazev = string.IsNullOrEmpty(m.Sekce.Nazev) ? m.Sekce.Oznaceni : m.Sekce.Nazev,
                          BookedDays = g.DaysCount
                      })
                .OrderByDescending(r => r.BookedDays)
                .ToList();

            var vm = new VytizenostViewModel
            {
                Od = odDatumCalc,
                Do = doDatumCalc,
                Rows = rows
            };

            return View(vm);
        }

        // Helper: zjistí aktuálního Zamestnanec podle claimù (email/upn/Identity.Name)
        private async Task<Zamestnanec?> GetCurrentZamestnanecAsync()
        {
            // 1) email
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                        ?? User.FindFirst("email")?.Value
                        ?? User.FindFirst("upn")?.Value;

            if (!string.IsNullOrEmpty(email))
            {
                var byEmail = await _context.Zamestnanci
                    .FirstOrDefaultAsync(z => z.Email != null && z.Email.ToLower() == email.ToLower());
                if (byEmail != null) return byEmail;
            }

            // 2) fallback na User.Identity.Name (napø. DOMAIN\user)
            var nameClaim = User.Identity?.Name;
            if (!string.IsNullOrEmpty(nameClaim))
            {
                var shortName = nameClaim.Contains("\\") ? nameClaim.Split('\\').Last() : nameClaim;
                // Zkusíme najít podle jména (èásteèné porovnání)
                var byName = await _context.Zamestnanci
                    .FirstOrDefaultAsync(z => z.Jmeno != null && z.Jmeno.Contains(shortName, StringComparison.OrdinalIgnoreCase));
                if (byName != null) return byName;
            }

            // nenalezeno
            return null;
        }
    }
}