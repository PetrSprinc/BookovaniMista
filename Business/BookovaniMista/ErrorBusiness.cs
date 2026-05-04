using Business.BookovaniMista.Resources;
using Entities.BookovaniMista.Models;

namespace Business.BookovaniMista
{
    public class ErrorBusiness
    {
        public static int GetHttpStatusCode(BookingErrorType errorType)
        {
            return errorType switch
            {
                BookingErrorType.SectionNotFound => 400,
                BookingErrorType.SeatNotFound => 400,
                BookingErrorType.UserNotFound => 401,
                BookingErrorType.DateInPast => 400,
                BookingErrorType.DateTooFar => 400,
                BookingErrorType.SeatAlreadyBooked => 409,
                BookingErrorType.DatabaseError => 500,
                BookingErrorType.ValidationFailed => 400,
                BookingErrorType.UnexpectedError => 500,
                BookingErrorType.AuthenticationFailed => 401,
                BookingErrorType.ModelValidationFailed => 400,
                BookingErrorType.ConcurrentBooking => 409,
                _ => 500
            };
        }
        public static string GetLocalizedMessage(BookingErrorType errorType, int maxDaysInFuture = 365)
        {
            return errorType switch
            {
                BookingErrorType.ValidationFailed => BookingErrorMessages.ValidationFailed,
                BookingErrorType.SectionNotFound => BookingErrorMessages.SectionNotFound,
                BookingErrorType.SeatNotFound => BookingErrorMessages.SeatNotFound,
                BookingErrorType.UserNotFound => BookingErrorMessages.UserNotFound,
                BookingErrorType.DateInPast => BookingErrorMessages.DateInPast,
                BookingErrorType.DateTooFar => string.Format(BookingErrorMessages.DateTooFarFuture, maxDaysInFuture),
                BookingErrorType.SeatAlreadyBooked => BookingErrorMessages.SeatAlreadyBooked,
                BookingErrorType.DatabaseError => BookingErrorMessages.DatabaseError,
                BookingErrorType.UnexpectedError => BookingErrorMessages.UnexpectedError,
                BookingErrorType.AuthenticationFailed => BookingErrorMessages.AuthenticationFailed,
                BookingErrorType.ModelValidationFailed => BookingErrorMessages.ModelValidationFailed,
                BookingErrorType.ConcurrentBooking => BookingErrorMessages.ConcurrentBooking,
                _ => BookingErrorMessages.UnexpectedError
            };
        }
    }
}
