using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Business.BookovaniMista.Interfaces;
using Business.BookovaniMista.Models;

namespace Business.BookovaniMista
{
    public class MapaBusiness : IMapaBusiness
    {
        private readonly BookovaniMistaDbContext _db;

        private const string MistoNumberSeparator = "-M";

        public MapaBusiness(BookovaniMistaDbContext db) => _db = db;

        public Task<IHtmlContent> RenderBookingForm(DateTime date) 
        {
            var today = date.ToString("yyyy-MM-dd");
            const int maxDaysInFuture = 365;
            var maxDate = date.AddDays(maxDaysInFuture).ToString("yyyy-MM-dd");
            var sb = new StringBuilder();

            sb.Append("<div class=\"card mb-4\">");
            sb.Append("<div class=\"card-body\">");
            sb.Append("<p>Klikněte na sekci mapy pro zobrazení detailu.</p>");

            sb.Append("<div class=\"mb-3\" style=\"max-width:260px;\">");
            sb.Append("<label for=\"booking-date\" class=\"form-label\">Vyberte den rezervace</label>");
            sb.Append($"<input id=\"booking-date\" name=\"bookingDate\" type=\"date\" class=\"form-control\" value=\"{WebUtility.HtmlEncode(today)}\" min=\"{WebUtility.HtmlEncode(today)}\" max=\"{WebUtility.HtmlEncode(maxDate)}\" required />");
            sb.Append("<div class=\"form-text\">Vybraný den bude odeslán spolu se žádostí o zabookování.</div>");
            sb.Append("</div>");

            sb.Append("<div class=\"ratio ratio-16x9\">");
            sb.Append($"<svg viewBox=\"{MapConfiguration.SvgViewBox}\" xmlns=\"{MapConfiguration.SvgNamespace}\" style=\"width:100%;height:100%;\">");

            // Renderovat všechny sekce z konfigurace
            foreach (var sekce in MapConfiguration.Sekce)
            {
                // Obdélník sekce
                sb.Append($"<a href=\"#{WebUtility.HtmlEncode(sekce.AnchorId)}\">");
                sb.Append($"<rect x=\"{sekce.X}\" y=\"{sekce.Y}\" width=\"{sekce.Width}\" height=\"{sekce.Height}\" class=\"section\" data-id=\"{sekce.Id}\" />");
                sb.Append("</a>");
            }

            // Štítky sekcí
            foreach (var sekce in MapConfiguration.Sekce)
            {
                sb.Append($"<a href=\"#{WebUtility.HtmlEncode(sekce.AnchorId)}\">");
                sb.Append($"<text x=\"{sekce.LabelX}\" y=\"{sekce.LabelY}\" text-anchor=\"middle\" class=\"label\">{WebUtility.HtmlEncode(sekce.Nazev)}</text>");
                sb.Append("</a>");
            }

            sb.Append("</svg>");
            sb.Append("</div>"); // ratio
            sb.Append("</div>"); // card-body
            sb.Append("</div>"); // card

            return Task.FromResult<IHtmlContent>(new HtmlString(sb.ToString()));
        }

        /// <summary>
        /// Renderuje HTML overlay sekce se seznamem míst (synchronní – pouze string building).
        /// Data o rezervacích musí být již předpočítána a předána jako parametr.
        /// </summary>
        public IHtmlContent RenderSectionOverlay(
            int sekceDbId, string anchorId, string title, string subtitle, 
            int total, int rows, DateTime date, string? currentUsername, 
            Dictionary<int, string?> bookedIndices)
        {
            if (string.IsNullOrEmpty(anchorId)) 
                throw new ArgumentException("anchorId is required", nameof(anchorId));
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
                        if (bookedIndices.TryGetValue(idx, out var reservujiciJmeno) && !string.IsNullOrEmpty(reservujiciJmeno))
                        {
                            // rezervováno
                            if (!string.IsNullOrEmpty(currentUsername) && string.Equals(reservujiciJmeno, currentUsername, StringComparison.OrdinalIgnoreCase))
                            {
                                sb.Append($"<button type=\"button\" class=\"btn seat booked-me\" data-seat=\"{idx}\" disabled>{idx}</button>");
                            }
                            else
                            {
                                // Obsazeno jiným -> červená
                                sb.Append($"<button type=\"button\" class=\"btn seat booked-other\" data-seat=\"{idx}\" disabled>{idx}</button>");
                            }
                        }
                        else
                        {
                            // Volné -> zelené
                            sb.Append($"<button type=\"button\" class=\"btn seat available\" data-seat=\"{idx}\">{idx}</button>");
                        }
                    }
                }
            }

            sb.Append("</div>"); // inner grid
            sb.Append("</div>"); // seats-grid
            sb.Append("</div>"); // detail
            sb.Append("</div>"); // overlay

            return new HtmlString(sb.ToString());
        }

        public Task<Dictionary<int, Dictionary<int, string?>>> GetAllBookingsForDateAsync(DateTime date, int[] sekceDbIds)
        {
            return GetAllReservationsForDateAsync(date, sekceDbIds);
        }

        public async Task<Dictionary<int, Dictionary<int, string?>>> GetAllReservationsForDateAsync(
            DateTime date, int[] seckeIds)
        {
            var d = date.Date;

            // Jeden dotaz pro všechny sekce najednou
            var rezervace = await (from r in _db.Rezervace
                                   join m in _db.Mista on r.MistoId equals m.Id
                                   join z in _db.Zamestnanci on r.ZamestnanecId equals z.Id
                                   where r.DatumRezervace >= d && 
                                         r.DatumRezervace < d.AddDays(1) && 
                                         seckeIds.Contains(m.SekceId)
                                   select new 
                                   { 
                                       m.SekceId, 
                                       m.Oznaceni, 
                                       m.Id, 
                                       ReservujiciJmeno = z.Jmeno 
                                   }).ToListAsync();

            // Organizovat data podle sekceId -> bookedIndices
            var result = new Dictionary<int, Dictionary<int, string?>>();

            foreach (var sekceId in seckeIds)
            {
                result[sekceId] = new Dictionary<int, string?>();
            }

            foreach (var item in rezervace)
            {
                if (string.IsNullOrEmpty(item.Oznaceni)) continue;
                
                var pos = item.Oznaceni.LastIndexOf(MistoNumberSeparator, StringComparison.OrdinalIgnoreCase);
                if (pos < 0) continue;
                
                var suffix = item.Oznaceni.Substring(pos + MistoNumberSeparator.Length);
                if (int.TryParse(suffix, out var idx) && idx > 0)
                {
                    if (result.ContainsKey(item.SekceId))
                    {
                        result[item.SekceId][idx] = item.ReservujiciJmeno;
                    }
                }
            }

            return result;
        }
    }
}
