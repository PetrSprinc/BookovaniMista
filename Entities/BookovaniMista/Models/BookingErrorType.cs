namespace Entities.BookovaniMista.Models
{
    /// <summary>
    /// Represents the type of error that occurred during booking
    /// </summary>
    public enum BookingErrorType
    {
        /// <summary>No error - booking was successful</summary>
        None = 0,

        /// <summary>Section (Sekce) not found</summary>
        SectionNotFound = 1,

        /// <summary>Seat (Misto) not found</summary>
        SeatNotFound = 2,

        /// <summary>User (Zamestnanec) not found</summary>
        UserNotFound = 3,

        /// <summary>Date is in the past</summary>
        DateInPast = 4,

        /// <summary>Date is too far in the future</summary>
        DateTooFar = 5,

        /// <summary>Seat is already booked for the requested date</summary>
        SeatAlreadyBooked = 6,

        /// <summary>Database error during save</summary>
        DatabaseError = 7,

        /// <summary>DTO validation failed</summary>
        ValidationFailed = 8,

        /// <summary>Unexpected error</summary>
        UnexpectedError = 9,

        /// <summary>User authentication failed</summary>
        AuthenticationFailed = 10,

        /// <summary>Model/DTO validation failed</summary>
        ModelValidationFailed = 11,

        /// <summary>Concurrent booking detected (race condition)</summary>
        ConcurrentBooking = 12
    }
}
