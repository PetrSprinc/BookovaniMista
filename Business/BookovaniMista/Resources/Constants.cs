namespace Business.BookovaniMista.Resources
{
    /// <summary>
    /// Centrální místo pro všechny konstant používané v aplikaci.
    /// Vyhýbáme se magic stringùm.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Názvy partial views pro Html.PartialAsync()
        /// </summary>
        public static class PartialViewNames
        {
            public const string BookingForm = "_BookingForm";
            public const string SeatGrid = "_SeatGrid";
            public const string SectionMap = "_SectionMap";
            public const string ConfirmDialog = "_ConfirmDialog";
        }

        /// <summary>
        /// Názvy API endpoints
        /// </summary>
        public static class ApiEndpoints
        {
            public const string Rezervovat = "/Akcni/Rezervovat";
        }

        /// <summary>
        /// Query string a form field names
        /// </summary>
        public static class RequestParameters
        {
            public const string BookingDate = "bookingDate";
        }

        /// <summary>
        /// HTML ID a CSS tøídy
        /// </summary>
        public static class HtmlIds
        {
            public const string BookingContainer = "booking-container";
        }

        /// <summary>
        /// ViewData klíèe pro pøedávání dat do views
        /// </summary>
        public static class ViewDataKeys
        {
            public const string Title = "Title";
            public const string CurrentUsername = "CurrentUsername";
        }
    }
}
