using Microsoft.AspNetCore.Html;

namespace Business.BookovaniMista.Interfaces
{
    public interface IMapaBusiness
    {
        Task<IHtmlContent> RenderMapCardAsync(DateTime date);
        Task<IHtmlContent> RenderSectionOverlayAsync(int sekceDbId, string anchorId, string title, string subtitle, int total, int rows, DateTime date, string? currentUsername);
    }
}