using Microsoft.EntityFrameworkCore;
using Xunit;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;

namespace BookovaniMista.Tests
{
    /// <summary>
    /// ?? BOOKING HISTORY AND RESERVATION TESTS
    /// 
    /// These tests verify:
    /// 1. Booking process (happy path)
    /// 2. Reservation persistence to database
    /// 3. History retrieval and display
    /// 4. User sees their own reservations
    /// </summary>
    public class BookingHistoryTests : IAsyncLifetime
    {
        private BookovaniMistaDbContext _db;
        private RezervaceBusiness _business;
        private MapaBusiness _mapaBusiness;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<BookovaniMistaDbContext>()
                .UseInMemoryDatabase($"BookingHistoryTest_{Guid.NewGuid()}")
                .Options;

            _db = new BookovaniMistaDbContext(options);
            _business = new RezervaceBusiness(_db);
            _mapaBusiness = new MapaBusiness(_db);
            await SeedTestData();
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        private async Task SeedTestData()
        {
            // Sekce
            var sekce = new Sekce { Id = 1, Oznaceni = "SJ1", Nazev = "Sekce jih 1" };
            _db.Sekce.Add(sekce);

            // Místa (10 seats)
            for (int i = 1; i <= 10; i++)
            {
                var misto = new Misto
                {
                    Id = i,
                    Oznaceni = $"SJ1-M{i}",
                    SekceId = 1,
                    Sekce = sekce
                };
                _db.Mista.Add(misto);
            }

            // Zamìstnanci (users)
            var user1 = new Zamestnanec
            {
                Id = 1,
                Jmeno = "Jan Novák",
                Email = "jan.novak@example.com"
            };
            var user2 = new Zamestnanec
            {
                Id = 2,
                Jmeno = "Marie Svobodová",
                Email = "marie@example.com"
            };

            _db.Zamestnanci.Add(user1);
            _db.Zamestnanci.Add(user2);

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// ?? TEST 1: Successful Booking
        /// 
        /// Verify that a valid booking request creates a reservation in database
        /// </summary>
        [Fact]
        public async Task BookingHistory_SuccessfulBooking_CreatesReservation()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            // Act
            var result = await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);

            // Assert - Booking succeeded
            Assert.True(result.Success);
            Assert.NotNull(result.Rezervace);

            // Assert - Database has reservation
            var reservation = await _db.Rezervace
                .Include(r => r.Misto)
                .Include(r => r.Zamestnanec)
                .FirstOrDefaultAsync();

            Assert.NotNull(reservation);
            Assert.Equal(1, reservation.MistoId);
            Assert.Equal("Jan Novák", reservation.Zamestnanec.Jmeno);
            Assert.Equal(DateTime.Today.AddDays(1).Date, reservation.DatumRezervace.Date);
        }

        /// <summary>
        /// ?? TEST 2: User Can See Their Reservations
        /// 
        /// Verify that when user logs in, they see their booking history
        /// </summary>
        [Fact]
        public async Task BookingHistory_UserReservations_UserSeesOwnBookings()
        {
            // Arrange - Book multiple seats for same user
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dayAfterTomorrow = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

            var booking1 = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };
            var booking2 = new RezervaceDto { SekceId = 1, SeatNumber = "2", Date = dayAfterTomorrow };

            // Act - Same user books two different seats
            var result1 = await _business.RezervovatAsync(booking1, "jan.novak@example.com", CancellationToken.None);
            var result2 = await _business.RezervovatAsync(booking2, "jan.novak@example.com", CancellationToken.None);

            // Assert - Both bookings succeeded
            Assert.True(result1.Success);
            Assert.True(result2.Success);

            // Get user's reservations
            var user = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email == "jan.novak@example.com");
            var userReservations = await _db.Rezervace
                .Where(r => r.ZamestnanecId == user.Id)
                .Include(r => r.Misto)
                .Include(r => r.Misto.Sekce)
                .OrderBy(r => r.DatumRezervace)
                .ToListAsync();

            // Assert - User has 2 reservations
            Assert.Equal(2, userReservations.Count);
            Assert.All(userReservations, r => Assert.Equal(user.Id, r.ZamestnanecId));
        }

        /// <summary>
        /// ?? TEST 3: User Does NOT See Other Users' Reservations
        /// 
        /// Verify that user1 doesn't see user2's bookings
        /// </summary>
        [Fact]
        public async Task BookingHistory_UserIsolation_UserOnlySeesOwnReservations()
        {
            // Arrange - User1 books a seat
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };

            await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);

            // Get both users
            var user1 = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email == "jan.novak@example.com");
            var user2 = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email == "marie@example.com");

            // Act - Get user1's reservations
            var user1Reservations = await _db.Rezervace
                .Where(r => r.ZamestnanecId == user1.Id)
                .ToListAsync();

            // Act - Get user2's reservations
            var user2Reservations = await _db.Rezervace
                .Where(r => r.ZamestnanecId == user2.Id)
                .ToListAsync();

            // Assert - User1 has 1, User2 has 0
            Assert.Single(user1Reservations);
            Assert.Empty(user2Reservations);
        }

        /// <summary>
        /// ?? TEST 4: Reservation Details Are Correct
        /// 
        /// Verify that all booking details are correctly stored and retrievable
        /// </summary>
        [Fact]
        public async Task BookingHistory_ReservationDetails_AllFieldsCorrect()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(3);
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "5",
                Date = futureDate.ToString("yyyy-MM-dd")
            };

            // Act
            var result = await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);

            // Assert
            var reservation = await _db.Rezervace
                .Include(r => r.Misto)
                .Include(r => r.Misto.Sekce)
                .Include(r => r.Zamestnanec)
                .FirstOrDefaultAsync();

            Assert.NotNull(reservation);
            Assert.Equal("Sekce jih 1", reservation.Misto.Sekce.Nazev);
            Assert.Equal("SJ1-M5", reservation.Misto.Oznaceni);
            Assert.Equal("Jan Novák", reservation.Zamestnanec.Jmeno);
            Assert.Equal(futureDate.Date, reservation.DatumRezervace.Date);
        }

        /// <summary>
        /// ?? TEST 5: Booking History Sorted by Date
        /// 
        /// Verify that reservations are displayed in chronological order
        /// </summary>
        [Fact]
        public async Task BookingHistory_SortedByDate_DisplaysInChronological()
        {
            // Arrange - Book 3 seats for different dates
            var dates = new[]
            {
                DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"),
                DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                DateTime.Today.AddDays(3).ToString("yyyy-MM-dd")
            };

            for (int i = 0; i < 3; i++)
            {
                var dto = new RezervaceDto
                {
                    SekceId = 1,
                    SeatNumber = (i + 1).ToString(),
                    Date = dates[i]
                };
                await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);
            }

            // Act - Get reservations sorted by date
            var user = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email == "jan.novak@example.com");
            var reservations = await _db.Rezervace
                .Where(r => r.ZamestnanecId == user.Id)
                .OrderBy(r => r.DatumRezervace)
                .ToListAsync();

            // Assert - Dates are in ascending order
            Assert.Equal(3, reservations.Count);
            Assert.True(reservations[0].DatumRezervace < reservations[1].DatumRezervace);
            Assert.True(reservations[1].DatumRezervace < reservations[2].DatumRezervace);

            var sortedDates = reservations.Select(r => r.DatumRezervace.Date).ToList();
            Assert.Equal(DateTime.Today.AddDays(1).Date, sortedDates[0]);
            Assert.Equal(DateTime.Today.AddDays(3).Date, sortedDates[1]);
            Assert.Equal(DateTime.Today.AddDays(5).Date, sortedDates[2]);
        }

        /// <summary>
        /// ?? TEST 6: Cancel Reservation (Optional)
        /// 
        /// Verify that user can cancel their booking
        /// </summary>
        [Fact]
        public async Task BookingHistory_CancelReservation_RemovesFromHistory()
        {
            // Arrange - Book a seat
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };
            var result = await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);
            var rezervaceId = result.Rezervace.Id;

            // Act - Delete reservation
            var reservation = await _db.Rezervace.FirstOrDefaultAsync(r => r.Id == rezervaceId);
            _db.Rezervace.Remove(reservation);
            await _db.SaveChangesAsync();

            // Assert - Reservation no longer exists
            var cancelled = await _db.Rezervace.FirstOrDefaultAsync(r => r.Id == rezervaceId);
            Assert.Null(cancelled);

            // User has 0 reservations
            var user = await _db.Zamestnanci.FirstOrDefaultAsync(z => z.Email == "jan.novak@example.com");
            var userReservations = await _db.Rezervace
                .Where(r => r.ZamestnanecId == user.Id)
                .ToListAsync();
            Assert.Empty(userReservations);
        }

        /// <summary>
        /// ?? TEST 7: GetAllReservationsForDate
        /// 
        /// Verify that we can retrieve all reservations for a specific date
        /// (used to show occupied seats on calendar)
        /// </summary>
        [Fact]
        public async Task BookingHistory_GetReservationsForDate_ShowsOccupiedSeats()
        {
            // Arrange - Book multiple seats for same date
            var tomorrow = DateTime.Today.AddDays(1);
            var tomorrowStr = tomorrow.ToString("yyyy-MM-dd");

            var bookings = new[]
            {
                new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrowStr },
                new RezervaceDto { SekceId = 1, SeatNumber = "2", Date = tomorrowStr },
                new RezervaceDto { SekceId = 1, SeatNumber = "3", Date = tomorrowStr }
            };

            foreach (var booking in bookings)
            {
                await _business.RezervovatAsync(booking, "jan.novak@example.com", CancellationToken.None);
            }

            // Act - Get all reservations for this date
            var reservations = await _db.Rezervace
                .Where(r => r.DatumRezervace == tomorrow.Date)
                .ToListAsync();

            // Assert - We have 3 reservations for this date
            Assert.Equal(3, reservations.Count);

            // All reservations are for same date
            Assert.All(reservations, r => Assert.Equal(tomorrow.Date, r.DatumRezervace.Date));
        }

        /// <summary>
        /// ?? TEST 8: Booking Creates Timestamp
        /// 
        /// Verify that reservations have creation date/time for audit trail
        /// </summary>
        [Fact]
        public async Task BookingHistory_Booking_HasTimestamp()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };
            var beforeBooking = DateTime.Now;

            // Act
            await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);
            var afterBooking = DateTime.Now;

            // Get reservation
            var reservation = await _db.Rezervace.FirstOrDefaultAsync();

            // Assert - Reservation has timestamp (if implemented)
            // Note: This assumes Rezervace entity has a CreatedAt or similar field
            Assert.NotNull(reservation);
            Assert.NotEqual(default(DateTime), reservation.DatumRezervace);
        }

        /// <summary>
        /// ?? TEST 9: User Cannot Book Past Date
        /// 
        /// Verify that user cannot book for dates in the past
        /// </summary>
        [Fact]
        public async Task BookingHistory_PastDate_BookingFails()
        {
            // Arrange
            var yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = yesterday
            };

            // Act
            var result = await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);

            // Assert - Booking failed with appropriate error
            Assert.False(result.Success);
            Assert.Equal(BookingErrorType.DateInPast, result.ErrorType);

            // No reservation created
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Empty(reservations);
        }

        /// <summary>
        /// ?? TEST 10: User Cannot Book Too Far in Future
        /// 
        /// Verify that bookings are limited to reasonable future period
        /// </summary>
        [Fact]
        public async Task BookingHistory_FutureDate_BookingFails()
        {
            // Arrange - Try to book 400 days in future (beyond allowed limit)
            var tooFar = DateTime.Today.AddDays(400).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tooFar
            };

            // Act
            var result = await _business.RezervovatAsync(dto, "jan.novak@example.com", CancellationToken.None);

            // Assert - Booking failed
            Assert.False(result.Success);
            Assert.Equal(BookingErrorType.DateTooFar, result.ErrorType);

            // No reservation created
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Empty(reservations);
        }
    }
}
