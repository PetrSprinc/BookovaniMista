using Microsoft.AspNetCore.Html;

namespace Business.BookovaniMista
{
    public interface IMapaBusiness
    {
        IHtmlContent RenderMapCard();
        IHtmlContent RenderSectionOverlay(int sekceDbId, string anchorId, string title, string subtitle, int total, int rows);
    }
}