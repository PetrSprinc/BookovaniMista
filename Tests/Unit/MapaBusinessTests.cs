using Microsoft.EntityFrameworkCore;
using Xunit;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;

namespace BookovaniMista.Tests.Unit
{
    /// <summary>
    /// Unit Tests for MapaBusiness class
    /// Tests for map rendering, reservation retrieval, and seat status
    /// </summary>
    public class MapaBusinessTests : IAsyncLifetime
    {
        private BookovaniMistaDbContext _db;
        private MapaBusiness _mapaBusiness;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<BookovaniMistaDbContext>()
                .UseInMemoryDatabase($"MapaBusinessTest_{Guid.NewGuid()}")
                .Options;

            _db = new BookovaniMistaDbContext(options);
            _mapaBusiness = new MapaBusiness(_db);
            await SeedTestData();
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        private async Task SeedTestData()
        {
            // Create sections
            var sekce = new List<Sekce>
            {
                new Sekce { Id = 1, Oznaceni = "SJ1", Nazev = "Sekce jih 1" },
                new Sekce { Id = 2, Oznaceni = "SJ2", Nazev = "Sekce jih 2" },
                new Sekce { Id = 3, Oznaceni = "SS1", Nazev = "Sekce sever 1" }
            };

            // Create seats
            var mista = new List<Misto>
            {
                new Misto { Id = 1, Oznaceni = "SJ1-M1", SekceId = 1, Sekce = sekce[0] },
                new Misto { Id = 2, Oznaceni = "SJ1-M2", SekceId = 1, Sekce = sekce[0] },
                new Misto { Id = 3, Oznaceni = "SJ1-M3", SekceId = 1, Sekce = sekce[0] },
                new Misto { Id = 4, Oznaceni = "SJ2-M1", SekceId = 2, Sekce = sekce[1] },
                new Misto { Id = 5, Oznaceni = "SS1-M1", SekceId = 3, Sekce = sekce[2] }
            };

            // Create employees
            var zamestnanci = new List<Zamestnanec>
            {
                new Zamestnanec { Id = 1, Jmeno = "John Smith", Email = "john@example.com" },
                new Zamestnanec { Id = 2, Jmeno = "Jane Doe", Email = "jane@example.com" }
            };

            // Create reservations
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var nextWeek = today.AddDays(7);

            var rezervace = new List<Rezervace>
            {
                new Rezervace { Id = 1, MistoId = 1, ZamestnanecId = 1, DatumRezervace = tomorrow, Misto = mista[0], Zamestnanec = zamestnanci[0] },
                new Rezervace { Id = 2, MistoId = 2, ZamestnanecId = 2, DatumRezervace = tomorrow, Misto = mista[1], Zamestnanec = zamestnanci[1] },
                new Rezervace { Id = 3, MistoId = 4, ZamestnanecId = 1, DatumRezervace = nextWeek, Misto = mista[3], Zamestnanec = zamestnanci[0] }
            };

            _db.Sekce.AddRange(sekce);
            _db.Mista.AddRange(mista);
            _db.Zamestnanci.AddRange(zamestnanci);
            _db.Rezervace.AddRange(rezervace);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Test: Render booking form for valid date
        /// Expected: HTML content generated successfully
        /// </summary>
        [Fact]
        public async Task RenderBookingForm_ValidDate_GeneratesHtml()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);

            // Act
            var result = await _mapaBusiness.RenderBookingForm(date);

            // Assert
            Assert.NotNull(result);
            var htmlString = result.ToString();
            Assert.NotEmpty(htmlString);
            Assert.Contains("svg", htmlString);
        }

        /// <summary>
        /// Test: Booking form includes section information
        /// Expected: Form contains section labels and IDs
        /// </summary>
        [Fact]
        public async Task RenderBookingForm_IncludesSectionData()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);

            // Act
            var result = await _mapaBusiness.RenderBookingForm(date);
            var htmlString = result.ToString();

            // Assert
            Assert.Contains("section-link", htmlString);
            Assert.Contains("label", htmlString);
        }

        /// <summary>
        /// Test: Get reservations for specific date and sections
        /// Expected: Only reservations for that date are returned
        /// </summary>
        [Fact]
        public async Task GetAllReservationsForDateAsync_SpecificDate_ReturnsCorrectReservations()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var sekceIds = new[] { 1, 2 };

            // Act
            var result = await _mapaBusiness.GetAllReservationsForDateAsync(date, sekceIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length); // 2 reservations for tomorrow in sections 1-2
            Assert.All(result, r => Assert.Equal(date, r.DatumRezervace));
        }

        /// <summary>
        /// Test: Get reservations with empty section list
        /// Expected: No reservations returned
        /// </summary>
        [Fact]
        public async Task GetAllReservationsForDateAsync_EmptySectionList_ReturnsEmpty()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var sekceIds = Array.Empty<int>();

            // Act
            var result = await _mapaBusiness.GetAllReservationsForDateAsync(date, sekceIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Test: Get reservations for date with no bookings
        /// Expected: Empty array returned
        /// </summary>
        [Fact]
        public async Task GetAllReservationsForDateAsync_NoReservations_ReturnsEmpty()
        {
            // Arrange
            var date = DateTime.Today.AddDays(100); // Far in future, no reservations
            var sekceIds = new[] { 1, 2, 3 };

            // Act
            var result = await _mapaBusiness.GetAllReservationsForDateAsync(date, sekceIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Test: Get reservations includes complete data
        /// Expected: Seat, employee, and section information included
        /// </summary>
        [Fact]
        public async Task GetAllReservationsForDateAsync_IncludesCompleteData()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var sekceIds = new[] { 1 };

            // Act
            var result = await _mapaBusiness.GetAllReservationsForDateAsync(date, sekceIds);

            // Assert
            Assert.NotEmpty(result);
            var reservation = result.First();
            Assert.NotNull(reservation.Misto);
            Assert.NotNull(reservation.Zamestnanec);
            Assert.NotNull(reservation.Misto.Sekce);
        }

        /// <summary>
        /// Test: Get available seats for specific section and date
        /// Expected: Only free seats returned
        /// </summary>
        [Fact]
        public async Task GetAvailableSeatsAsync_SpecificSection_ReturnsFreeSeats()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var sekceId = 1;

            // Act
            var result = await _mapaBusiness.GetAvailableSeatsAsync(sekceId, date);

            // Assert
            Assert.NotNull(result);
            // Section 1 has 3 seats, 2 are booked
            Assert.Equal(1, result.Count()); // Only 1 free seat
            Assert.Equal(3, result.First().Id); // Seat 3 is free
        }

        /// <summary>
        /// Test: Get available seats for date with no bookings
        /// Expected: All seats are available
        /// </summary>
        [Fact]
        public async Task GetAvailableSeatsAsync_NoBooksForDate_ReturnsAllSeats()
        {
            // Arrange
            var date = DateTime.Today.AddDays(100); // No reservations
            var sekceId = 1;

            // Act
            var result = await _mapaBusiness.GetAvailableSeatsAsync(sekceId, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count()); // All 3 seats in section 1 are free
        }

        /// <summary>
        /// Test: Get reserved seats for specific date
        /// Expected: Only booked seats returned
        /// </summary>
        [Fact]
        public async Task GetReservedSeatsAsync_SpecificDate_ReturnsBookedSeats()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var sekceId = 1;

            // Act
            var result = await _mapaBusiness.GetReservedSeatsAsync(sekceId, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // 2 seats booked
            Assert.True(result.Any(r => r.Id == 1));
            Assert.True(result.Any(r => r.Id == 2));
        }

        /// <summary>
        /// Test: Get reserved seats for date with no bookings
        /// Expected: Empty list
        /// </summary>
        [Fact]
        public async Task GetReservedSeatsAsync_NoBookings_ReturnsEmpty()
        {
            // Arrange
            var date = DateTime.Today.AddDays(100);
            var sekceId = 1;

            // Act
            var result = await _mapaBusiness.GetReservedSeatsAsync(sekceId, date);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Test: Verify section borders are correctly calculated
        /// Expected: X, Y, width, height are properly set
        /// </summary>
        [Fact]
        public void MapConfiguration_SectionBorders_AreValid()
        {
            // Assert
            Assert.NotEmpty(Business.BookovaniMista.Resources.MapConfiguration.Sekce);
            foreach (var section in Business.BookovaniMista.Resources.MapConfiguration.Sekce)
            {
                Assert.True(section.X >= 0, "X coordinate must be non-negative");
                Assert.True(section.Y >= 0, "Y coordinate must be non-negative");
                Assert.True(section.Width > 0, "Width must be positive");
                Assert.True(section.Height > 0, "Height must be positive");
                Assert.NotEmpty(section.Nazev);
            }
        }
    }
}
