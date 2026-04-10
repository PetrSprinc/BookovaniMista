using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista.Interfaces;

namespace BookovaniMista.Controllers
{
    [Route("Akcni")]
    public partial class AkcniController : Controller
    {
        private readonly ILogger<AkcniController> _logger;
        //private readonly BookovaniMistaDbContext _db;
        private readonly IRezervaceBusiness _rezervaceBusiness;

        //public AkcniController(ILogger<AkcniController> logger, BookovaniMistaDbContext db, IRezervaceBusiness rezervaceBusiness)
        public AkcniController(ILogger<AkcniController> logger, IRezervaceBusiness rezervaceBusiness)
        {
            _logger = logger;
            //_db = db;
            _rezervaceBusiness = rezervaceBusiness;
        }

        // Pøedáváme username (pokud ho máme) do view pomocí ViewData
        public async Task<IActionResult> Zabookovat()
        {
            string? currentUsername = User.FindFirst(ClaimTypes.Email)?.Value
                                      ?? User.Identity?.Name;
            ViewData["CurrentUsername"] = currentUsername;
            await Task.CompletedTask; // Pøidáno pro odstranìní CS1998
            return View();
        }

        // POST /Akcni/Rezervovat
        [HttpPost("Rezervovat")]
        public async Task<IActionResult> Rezervovat([FromBody] RezervaceDto dto, CancellationToken ct)
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Unauthorized();
            var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? string.Empty;

            var (success, error, rezervace) = await _rezervaceBusiness.RezervovatAsync(dto, userIdentifier, ct);
            if (!success)
            {
                if (error?.Contains("nenalezena") == true) return BadRequest(error);
                if (error?.Contains("Kolize") == true) return Conflict(error);
                return StatusCode(500, error);
            }

            return Ok(new { success = true, rezervaceId = rezervace!.Id, mistoId = rezervace.MistoId, sekceId = rezervace.Misto?.Sekce?.Id, date = rezervace.DatumRezervace.ToString("yyyy-MM-dd") });
        }
    }
}