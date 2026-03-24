using System.Diagnostics;
using BookovaniMista.Models;
using Microsoft.AspNetCore.Mvc;
using Entities.BookovaniMista;
using Microsoft.EntityFrameworkCore;

namespace BookovaniMista.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookovaniMistaDbContext _db;

        public HomeController(ILogger<HomeController> logger, BookovaniMistaDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var vm = new ViewModel
            {
                Sekce = await _db.Sekce.OrderBy(s => s.Id).ToListAsync(),
                Mista = await _db.Mista.OrderBy(m => m.Id).ToListAsync(),
                Zamestnanci = await _db.Zamestnanci.OrderBy(z => z.Id).ToListAsync(),
                Rezervace = await _db.Rezervace.OrderBy(r => r.Id).ToListAsync()
            };

            return View(vm);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //seøazení listù podle ID pro zobrazení
        public async Task<IActionResult> Sekce()
        {
            var serazenyList = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(serazenyList);
        }
        public async Task<IActionResult> Mista()
        {
            var serazenyList = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(serazenyList);
        }
        public async Task<IActionResult> Zamestnanci()
        {
            var serazenyList = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(serazenyList);
        }
        public async Task<IActionResult> Rezervace()
        {
            var serazenyList = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(serazenyList);
        }
    }
}
