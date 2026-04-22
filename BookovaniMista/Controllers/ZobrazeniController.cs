using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista.ViewModels;
using Business.BookovaniMista.Interfaces;

namespace BookovaniMista.Controllers
{
    public class ZobrazeniController : Controller
    {
        private readonly ILogger<ZobrazeniController> _logger;
        private readonly BookovaniMistaDbContext _db;
        private readonly ICommonBusiness _commonBusiness;

        public ZobrazeniController(ILogger<ZobrazeniController> logger, BookovaniMistaDbContext db, ICommonBusiness commonBusiness)
        {
            _logger = logger;
            _db = db;
            _commonBusiness = commonBusiness;
        }

        // Historie: zobrazí pouze rezervace vytvoøené pøihlášeným uživatelem (max 100 posledních)
        public async Task<IActionResult> Historie()
        {
            try
            {
                // Najít aktuálního zamìstnance podle claimu (email / upn / User.Identity.Name)
                var zam = await _commonBusiness.GetCurrentZamestnanecAsync(User);
                if (zam == null)
                {
                    _logger.LogWarning("User not found in database");
                    ViewData["ErrorMessage"] = "Nepodaøilo se identifikovat uživatele v DB (zkontrolujte email).";
                    return View(new List<Rezervace>());
                }

                // Naèíst posledních 100 rezervací tohoto zamìstnance (s místem a sekcí)
                var rezervace = await _db.Rezervace
                    .Where(r => r.ZamestnanecId == zam.Id)
                    .Include(r => r.Misto)
                        .ThenInclude(m => m.Sekce)
                    .OrderByDescending(r => r.DatumRezervace)
                    .Take(100)
                    .ToListAsync();

                return View(rezervace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Historie");
                ViewData["ErrorMessage"] = "Chyba pøi naèítání historie.";
                return View(new List<Rezervace>());
            }
        }

        public IActionResult Obsazenost()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Obsazenost");
                return RedirectToAction("Error", "Home");
            }
        }

        // Vytíženost: volitelnì od/do (GET), výchozí od zaèátku mìsíce do jeho konce
        public async Task<IActionResult> Vytizenost(DateTime? odDatum, DateTime? doDatum)
        {
            try
            {
                var result = await _commonBusiness.GetVytizenostAsync(odDatum, doDatum);

                // mapovat na ViewModel použitý ve view
                var vm = new VytizenostResult
                {
                    Od = result.Od,
                    Do = result.Do,
                    Rows = result.Rows.Select(r => new VytizenostRow
                    {
                        MistoOznaceni = r.MistoOznaceni,
                        SekceNazev = r.SekceNazev,
                        BookedDays = r.BookedDays
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Vytizenost");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}