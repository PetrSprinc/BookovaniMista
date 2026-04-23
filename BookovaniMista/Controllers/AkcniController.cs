using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista.Interfaces;
using Business.BookovaniMista;

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

        // GET /Akcni/Zabookovat
        public IActionResult Zabookovat()
        {
            try
            {
                string? currentUsername = User.FindFirst(ClaimTypes.Email)?.Value
                                          ?? User.Identity?.Name;
                ViewData["CurrentUsername"] = currentUsername;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zabookovat page");
                return RedirectToAction("Error", "Home");
            }
        }

        // POST /Akcni/Rezervovat
        [HttpPost("Rezervovat")]
        public async Task<IActionResult> Rezervovat([FromBody] RezervaceDto dto, CancellationToken ct)
        {
            try
            {
                // Authentication check
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("Booking attempt by unauthenticated user");
                    return Unauthorized(new { success = false, error = "User not authenticated" });
                }

                var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value 
                                    ?? User.Identity?.Name 
                                    ?? string.Empty;

                if (string.IsNullOrEmpty(userIdentifier))
                {
                    _logger.LogWarning("Booking attempt with empty user identifier");
                    return Unauthorized(new { success = false, error = "User not authenticated" });
                }

                // Model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Booking validation failed: {Errors}", string.Join("; ", errors));
                    return BadRequest(new { success = false, error = "Invalid booking data" });
                }

                // Business logic
                var result = await _rezervaceBusiness.RezervovatAsync(dto, userIdentifier, ct);

                if (!result.Success)
                {
                    int statusCode = ErrorBusiness.GetHttpStatusCode(result.ErrorType);

                    _logger.LogWarning("Booking failed for user {User} - ErrorType: {ErrorType}, Message: {Message}",
                        userIdentifier, result.ErrorType, result.ErrorMessage);

                    return StatusCode(statusCode, new { success = false, error = result.ErrorMessage });
                }

                _logger.LogInformation(
                    "Booking successful - User: {User}, Reservation ID: {RezervaceId}, Section: {SectionId}, Seat: {SeatNumber}, Date: {Date}",
                    userIdentifier, result.Rezervace!.Id, result.Rezervace.Misto?.Sekce?.Id, dto.SeatNumber, dto.Date);

                return Ok(new
                {
                    success = true,
                    rezervaceId = result.Rezervace.Id,
                    mistoId = result.Rezervace.MistoId,
                    sekceId = result.Rezervace.Misto?.Sekce?.Id,
                    date = result.Rezervace.DatumRezervace.ToString("yyyy-MM-dd")
                });
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Race condition: two concurrent bookings
                _logger.LogWarning(ex, "Race condition detected - concurrent booking on same seat. User: {User}", 
                    User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name);
                return Conflict(new { success = false, error = "Seat was just booked by another user. Please refresh and try again." });
            }
            catch (DbUpdateException ex)
            {
                // Other database errors
                _logger.LogError(ex, "Database error during booking");
                return StatusCode(500, new { success = false, error = "Database error occurred. Please try again." });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Booking request cancelled");
                return StatusCode(499, new { success = false, error = "Request was cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during booking. Exception type: {ExceptionType}", ex.GetType().Name);
                return StatusCode(500, new { success = false, error = "An unexpected error occurred. Please try again." });
            }
        }
    }
}