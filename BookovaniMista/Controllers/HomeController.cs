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

        public async Task<IActionResult> Index()
        {
            var sekce = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(sekce);
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

        //pro view listu z db sekce
        public async Task<IActionResult> Sekce()
        {
            var sekce = await _db.Sekce.OrderBy(s => s.Id).ToListAsync();
            return View(sekce);
        }
    }
}
