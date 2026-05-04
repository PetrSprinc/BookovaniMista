using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Business.BookovaniMista.Interfaces;
using Business.BookovaniMista.Resources;

namespace Business.BookovaniMista
{
    /// <summary>
    /// Logika mapy - řídí vykreslování interaktivní mapy a správu rezervací.
    /// Využívá String.Join + LINQ místo StringBuilder pro čistší a čitelnější HTML generování.
    /// </summary>
    public class MapaBusiness : IMapaBusiness
    {
        private readonly BookovaniMistaDbContext _db;

        private const string MistoNumberSeparator = "-M";

        public MapaBusiness(BookovaniMistaDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Vykresluje formulář pro rezervaci s výběrem data a interaktivní SVG mapou.
        /// Využívá String.Join + String Interpolation místo StringBuilder.
        /// </summary>
        public Task<IHtmlContent> RenderBookingForm(DateTime date)
        {
            var today = date.ToString("yyyy-MM-dd");
            const int maxDaysInFuture = 365;
            var maxDate = date.AddDays(maxDaysInFuture).ToString("yyyy-MM-dd");

            // Generování prvků sekcí pomocí String.Join + LINQ
            var sectionRects = string.Join("\n", 
                MapConfiguration.Sekce.Select(s => $@"
        <a href=""#{WebUtility.HtmlEncode(s.AnchorId)}"" class=""section-link"" data-sekce-db=""{s.Id}"">
            <rect x=""{s.X}"" y=""{s.Y}"" width=""{s.Width}"" height=""{s.Height}"" 
                  class=""section"" data-id=""{s.Id}"" />
        </a>"));

            var sectionLabels = string.Join("\n", 
                MapConfiguration.Sekce.Select(s => $@"
        <text x=""{s.LabelX}"" y=""{s.LabelY}"" text-anchor=""middle"" class=""label"">{WebUtility.HtmlEncode(s.Nazev)}</text>"));

            // Sestavení kompletního HTML pomocí String Interpolation
            var html = $@"
<div class=""card mb-4"">
    <div class=""card-body"">
        <p>Klikněte na sekci mapy pro zobrazení detailu.</p>

        <div class=""mb-3"" style=""max-width:260px;"">
            <label for=""booking-date"" class=""form-label"">Vyberte den rezervace</label>
            <input id=""booking-date"" name=""bookingDate"" type=""date"" class=""form-control""
                   value=""{WebUtility.HtmlEncode(today)}"" 
                   min=""{WebUtility.HtmlEncode(today)}"" 
                   max=""{WebUtility.HtmlEncode(maxDate)}"" 
                   required />
            <div class=""form-text"">Vybraný den bude odeslán spolu se žádostí o zabookování.</div>
        </div>

        <div class=""ratio ratio-16x9"">
            <svg viewBox=""{MapConfiguration.SvgViewBox}"" 
                 xmlns=""{MapConfiguration.SvgNamespace}"" 
                 style=""width:100%;height:100%;"">
                {sectionRects}
                {sectionLabels}
            </svg>
        </div>
    </div>
</div>";

            return Task.FromResult<IHtmlContent>(new HtmlString(html));
        }

        /// <summary>
        /// Vykresluje overlay sekce se všemi místy.
        /// Využívá String.Join + LINQ pro generování tlačítek míst.
        /// </summary>
        public IHtmlContent RenderSectionOverlay(
            int sekceDbId, string anchorId, string title, string subtitle,
            int total, int rows, DateTime date, string? currentUsername,
            Dictionary<int, string?> bookedIndices)
        {
            if (string.IsNullOrEmpty(anchorId))
                throw new ArgumentException("anchorId is required", nameof(anchorId));
            if (rows <= 0) rows = 1;

            // Generate seat buttons using String.Join + LINQ
            var seatsButtons = GenerateSeatsButtons(total, rows, currentUsername, bookedIndices);

            var html = $@"
<div id=""{WebUtility.HtmlEncode(anchorId)}"" class=""overlay"" aria-hidden=""true"" data-sekce-db=""{sekceDbId}"">
    <div class=""detail"">
        <a class=""close"" href=""#"" aria-label=""Zavřít"">&times;</a>
        <h3>{WebUtility.HtmlEncode(title)}</h3>
        <p class=""muted"">{WebUtility.HtmlEncode(subtitle)}</p>
        {seatsButtons}
    </div>
</div>";

            return new HtmlString(html);
        }

        /// <summary>
        /// Render only the seats grid (for lazy loading / AJAX requests)
        /// </summary>
        public IHtmlContent RenderSeatsGrid(
            int sekceDbId, string title, int total, int rows,
            DateTime date, string? currentUsername, Dictionary<int, string?> bookedIndices)
        {
            return new HtmlString(GenerateSeatsButtons(total, rows, currentUsername, bookedIndices));
        }

        /// <summary>
        /// Generate seats grid HTML using String.Join + LINQ (shared helper)
        /// This is the key method demonstrating String.Join + LINQ approach
        /// </summary>
        private string GenerateSeatsButtons(
            int total, int rows, string? currentUsername, Dictionary<int, string?> bookedIndices)
        {
            if (rows <= 0) rows = 1;
            var cols = (int)Math.Ceiling(total / (double)rows);

            // Generate all seat buttons using LINQ
            var seatButtons = Enumerable.Range(1, total)
                .Select(seatIndex =>
                {
                    bool isBooked = bookedIndices.TryGetValue(seatIndex, out var bookedByUser) && !string.IsNullOrEmpty(bookedByUser);

                    if (isBooked)
                    {
                        bool isBookedByMe = !string.IsNullOrEmpty(currentUsername) &&
                                           string.Equals(bookedByUser, currentUsername, StringComparison.OrdinalIgnoreCase);

                        var cssClass = isBookedByMe ? "booked-me" : "booked-other";
                        return $"<button type=\"button\" class=\"btn seat {cssClass}\" data-seat=\"{seatIndex}\" disabled>{seatIndex}</button>";
                    }
                    else
                    {
                        return $"<button type=\"button\" class=\"btn seat available\" data-seat=\"{seatIndex}\">{seatIndex}</button>";
                    }
                });

            // Use String.Join to combine all buttons
            var seatsGridContent = string.Join("\n", seatButtons);

            // Wrap in grid container using String Interpolation
            return $@"
<div class=""seats-grid"">
    <div style=""display:grid; grid-template-columns:repeat({cols}, 1fr); gap:.5rem;"">
        {seatsGridContent}
    </div>
</div>";
        }

        /// <summary>
        /// Get all reservations for specified sections on a given date
        /// Returns dictionary: sectionId -> (seatIndex -> bookedByUserName)
        /// </summary>
        public async Task<Dictionary<int, Dictionary<int, string?>>> GetAllReservationsForDateAsync(
            DateTime date, int[] sekceDbIds)
        {
            var d = date.Date;

            var rezervace = await (from r in _db.Rezervace
                                   join m in _db.Mista on r.MistoId equals m.Id
                                   join z in _db.Zamestnanci on r.ZamestnanecId equals z.Id
                                   where r.DatumRezervace >= d &&
                                         r.DatumRezervace < d.AddDays(1) &&
                                         sekceDbIds.Contains(m.SekceId)
                                   select new
                                   {
                                       m.SekceId,
                                       m.Oznaceni,
                                       m.Id,
                                       ReservujiciJmeno = z.Jmeno
                                   }).ToListAsync();

            // Organize data by sectionId -> bookedIndices using LINQ ToDictionary
            var result = sekceDbIds.ToDictionary(
                sekceId => sekceId,
                sekceId => new Dictionary<int, string?>()
            );

            foreach (var item in rezervace)
            {
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
