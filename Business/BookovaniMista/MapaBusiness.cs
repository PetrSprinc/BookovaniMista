using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Html;

namespace Business.BookovaniMista
{
    public class MapaBusiness : IMapaBusiness
    {
        public IHtmlContent RenderMapCard()
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var sb = new StringBuilder();

            sb.Append("<div class=\"card mb-4\">");
            sb.Append("<div class=\"card-body\">");
            sb.Append("<p>Klikněte na sekci mapy pro zobrazení detailu.</p>");

            sb.Append("<div class=\"mb-3\" style=\"max-width:260px;\">");
            sb.Append("<label for=\"booking-date\" class=\"form-label\">Vyberte den rezervace</label>");
            sb.Append($"<input id=\"booking-date\" name=\"bookingDate\" type=\"date\" class=\"form-control\" value=\"{WebUtility.HtmlEncode(today)}\" />");
            sb.Append("<div class=\"form-text\">Vybraný den bude odeslán spolu se žádostí o zabookování.</div>");
            sb.Append("</div>");

            sb.Append("<div class=\"ratio ratio-16x9\">");
            sb.Append("<svg viewBox=\"0 0 1000 500\" xmlns=\"http://www.w3.org/2000/svg\" style=\"width:100%;height:100%;\">");

            // Horní řada: 4 sekce
            sb.Append("<a href=\"#SJ1\"><rect x=\"20\"  y=\"20\"  width=\"220\" height=\"200\" class=\"section\" data-id=\"1\" /></a>");
            sb.Append("<a href=\"#SJ2\"><rect x=\"260\" y=\"20\"  width=\"220\" height=\"200\" class=\"section\" data-id=\"2\" /></a>");
            sb.Append("<a href=\"#SJ3\"><rect x=\"500\" y=\"20\"  width=\"220\" height=\"200\" class=\"section\" data-id=\"3\" /></a>");
            sb.Append("<a href=\"#SJ4\"><rect x=\"740\" y=\"20\"  width=\"220\" height=\"200\" class=\"section\" data-id=\"4\" /></a>");

            // Dolní řada: 5 sekcí
            sb.Append("<a href=\"#SS1\"><rect x=\"20\"  y=\"260\" width=\"150\" height=\"200\" class=\"section\" data-id=\"5\" /></a>");
            sb.Append("<a href=\"#SS2\"><rect x=\"190\" y=\"260\" width=\"150\" height=\"200\" class=\"section\" data-id=\"6\" /></a>");
            sb.Append("<a href=\"#SS3\"><rect x=\"360\" y=\"260\" width=\"150\" height=\"200\" class=\"section\" data-id=\"7\" /></a>");
            sb.Append("<a href=\"#SS4\"><rect x=\"530\" y=\"260\" width=\"150\" height=\"200\" class=\"section\" data-id=\"8\" /></a>");
            sb.Append("<a href=\"#SS5\"><rect x=\"700\" y=\"260\" width=\"150\" height=\"200\" class=\"section\" data-id=\"9\" /></a>");

            // Štítky
            sb.Append("<a href=\"#SJ1\"><text x=\"130\" y=\"120\" text-anchor=\"middle\" class=\"label\">Sekce jih 1</text></a>");
            sb.Append("<a href=\"#SJ2\"><text x=\"370\" y=\"120\" text-anchor=\"middle\" class=\"label\">Sekce jih 2</text></a>");
            sb.Append("<a href=\"#SJ3\"><text x=\"610\" y=\"120\" text-anchor=\"middle\" class=\"label\">Sekce jih 3</text></a>");
            sb.Append("<a href=\"#SJ4\"><text x=\"850\" y=\"120\" text-anchor=\"middle\" class=\"label\">Sekce jih 4</text></a>");

            sb.Append("<a href=\"#SS1\"><text x=\"95\" y=\"360\" text-anchor=\"middle\" class=\"label\">Sekce sever 1</text></a>");
            sb.Append("<a href=\"#SS2\"><text x=\"265\" y=\"360\" text-anchor=\"middle\" class=\"label\">Sekce sever 2</text></a>");
            sb.Append("<a href=\"#SS3\"><text x=\"435\" y=\"360\" text-anchor=\"middle\" class=\"label\">Sekce sever 3</text></a>");
            sb.Append("<a href=\"#SS4\"><text x=\"605\" y=\"360\" text-anchor=\"middle\" class=\"label\">Sekce sever 4</text></a>");
            sb.Append("<a href=\"#SS5\"><text x=\"775\" y=\"360\" text-anchor=\"middle\" class=\"label\">Sekce sever 5</text></a>");

            sb.Append("</svg>");
            sb.Append("</div>"); // ratio
            sb.Append("</div>"); // card-body
            sb.Append("</div>"); // card

            return new HtmlString(sb.ToString());
        }
        public IHtmlContent RenderSectionOverlay(int sekceDbId, string anchorId, string title, string subtitle, int total, int rows)
        {
            if (string.IsNullOrEmpty(anchorId)) throw new ArgumentException("anchorId is required", nameof(anchorId));
            if (rows <= 0) rows = 1;
            var cols = (int)Math.Ceiling(total / (double)rows);

            var sb = new StringBuilder();

            sb.Append($"<div id=\"{WebUtility.HtmlEncode(anchorId)}\" class=\"overlay\" aria-hidden=\"true\" data-sekce-db=\"{sekceDbId}\">");
            sb.Append("<div class=\"detail\">");
            sb.Append("<a class=\"close\" href=\"#\" aria-label=\"Zavřít\">&times;</a>");
            sb.Append($"<h3>{WebUtility.HtmlEncode(title)}</h3>");
            sb.Append($"<p class=\"muted\">{WebUtility.HtmlEncode(subtitle)}</p>");
            sb.Append("<div class=\"seats-grid\">");

            sb.Append($"<div style=\"display:grid; grid-template-columns:repeat({cols}, 1fr); gap:.5rem;\">");

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var idx = r * cols + c + 1;
                    if (idx <= total)
                    {
                        sb.Append($"<button type=\"button\" class=\"btn btn-outline-primary seat\" data-seat=\"{idx}\">{idx}</button>");
                    }
                }
            }

            sb.Append("</div>"); // inner grid
            sb.Append("</div>"); // seats-grid
            sb.Append("</div>"); // detail
            sb.Append("</div>"); // overlay

            return new HtmlString(sb.ToString());
        }
    }
}
