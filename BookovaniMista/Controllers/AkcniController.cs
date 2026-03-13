using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BookovaniMista.Controllers
{
    public class AkcniController : Controller
    {
        private readonly ILogger<AkcniController> _logger;

        public AkcniController(ILogger<AkcniController> logger)
        {
            _logger = logger;
        }

        public IActionResult Zabookovat()
        {
            return View();
        }
    }
}