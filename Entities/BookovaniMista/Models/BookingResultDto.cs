namespace Entities.BookovaniMista.Models
{
    /// <summary>
    /// Data transfer object for booking operation results
    /// Contains success flag, error details, and the created reservation if successful
    /// </summary>
    public class BookingResultDto
    {
        public bool Success { get; set; }
        public BookingErrorType ErrorType { get; set; } = BookingErrorType.None;
        public string? ErrorMessage { get; set; }
        public Rezervace? Rezervace { get; set; }
    }
}
