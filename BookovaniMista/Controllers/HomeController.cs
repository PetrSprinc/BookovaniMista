using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Index page");
                return RedirectToAction(nameof(Error));
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Privacy page");
                return RedirectToAction(nameof(Error));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
