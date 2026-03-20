using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public IActionResult Historie()
        {
            var vseSekce = _context.Sekce.ToList();

            return View(vseSekce);
        }
        public IActionResult Obsazenost()
        {
            return View();
        }
        public IActionResult Vytizenost()
        {
            return View();
        }
        public IActionResult PridatSekci()
        {
            return View();
        }
    }
}