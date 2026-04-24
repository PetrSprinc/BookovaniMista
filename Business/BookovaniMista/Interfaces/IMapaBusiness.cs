using Microsoft.AspNetCore.Html;

namespace Business.BookovaniMista.Interfaces
{
    public interface IMapaBusiness
    {
        Task<IHtmlContent> RenderBookingForm(DateTime date);

        IHtmlContent RenderSectionOverlay(
            int sekceDbId, string anchorId, string title, string subtitle, 
            int total, int rows, DateTime date, string? currentUsername, 
            Dictionary<int, string?> bookedIndices);

        /// <summary>
        /// Render only the seats grid (for lazy loading / AJAX)
        /// </summary>
        IHtmlContent RenderSeatsGrid(
            int sekceDbId, string title, int total, int rows,
            DateTime date, string? currentUsername, Dictionary<int, string?> bookedIndices);

        /// <summary>
        /// Render confirmation dialog for booking
        /// </summary>
        IHtmlContent RenderConfirmDialog();

        Task<Dictionary<int, Dictionary<int, string?>>> GetAllReservationsForDateAsync(
            DateTime date, int[] sekceDbIds);
    }
}
