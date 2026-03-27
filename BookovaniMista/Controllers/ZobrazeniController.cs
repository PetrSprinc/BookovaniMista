using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BookovaniMista.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;

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
        public IActionResult Historie()
        {
            return View();
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
    }
}