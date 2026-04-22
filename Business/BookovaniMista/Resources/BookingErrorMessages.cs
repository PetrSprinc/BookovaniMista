namespace Business.BookovaniMista.Resources
{
    /// <summary>
    /// Localized error messages for booking operations
    /// This class is designed to be easily replaceable for different languages
    /// </summary>
    public static class BookingErrorMessages
    {
        // Validation Errors
        public const string ValidationFailed = "Data rezervace nejsou platná.";

        // Section Errors
        public const string SectionNotFound = "Sekce nenalezena.";

        // Seat Errors
        public const string SeatNotFound = "Místo nenalezeno pro zadanou sekci/èíslo.";

        // User Errors
        public const string UserNotFound = "Pøihlášený uživatel nenalezen v DB.";

        // Date Errors
        public const string DateInPast = "Nelze zarezervovat místo v minulosti.";
        public const string DateTooFarFuture = "Nelze zarezervovat místo více než {0} dní dopøedu.";

        // Booking Errors
        public const string SeatAlreadyBooked = "Místo již zarezervované pro zvolený den.";

        // Database Errors
        public const string DatabaseError = "Chyba pøi ukládání rezervace (možná kolize).";
        public const string DatabaseConnectionError = "Chyba pøipojení k databázi.";

        // Generic Errors
        public const string UnexpectedError = "Nastala neoèekávaná chyba. Prosím, zkuste znovu.";
        public const string OperationCancelled = "Operace byla zrušena.";
    }
}
