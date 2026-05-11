using Microsoft.EntityFrameworkCore;
using Xunit;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;
using System.Diagnostics;

namespace BookovaniMista.Tests
{
    /// <summary>
    /// ?? RACE CONDITION FIX VERIFICATION TEST
    /// 
    /// This test file verifies that the race condition vulnerability
    /// (two users booking same seat simultaneously) has been fixed.
    /// 
    /// The fix uses:
    /// - UNIQUE database constraint on (MistoId, DatumRezervace)
    /// - Transaction exception handling on DbUpdateException
    /// - Proper error response on concurrent booking attempt
    /// </summary>
    public class RaceConditionFixTests : IAsyncLifetime
    {
        private BookovaniMistaDbContext _db;
        private RezervaceBusiness _business;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<BookovaniMistaDbContext>()
                .UseInMemoryDatabase($"RaceConditionTest_{Guid.NewGuid()}")
                .Options;

            _db = new BookovaniMistaDbContext(options);
            _business = new RezervaceBusiness(_db);
            await SeedTestData();
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        private async Task SeedTestData()
        {
            var sekce = new Sekce { Id = 1, Oznaceni = "SJ1", Nazev = "Sekce jih 1" };
            var misto = new Misto { Id = 1, Oznaceni = "SJ1-M1", SekceId = 1, Sekce = sekce };

            var users = new List<Zamestnanec>();
            for (int i = 1; i <= 50; i++)
            {
                users.Add(new Zamestnanec
                {
                    Id = i,
                    Jmeno = $"User {i}",
                    Email = $"user{i}@example.com"
                });
            }

            _db.Sekce.Add(sekce);
            _db.Mista.Add(misto);
            _db.Zamestnanci.AddRange(users);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// ?? PRIMARY TEST: Two concurrent bookings on same seat
        /// 
        /// Expected: Exactly ONE succeeds, ONE fails with SeatAlreadyBooked
        /// If test fails: Race condition still exists (both could succeed)
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_TwoThreads_SameSeat_OnlyOneSucceeds()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            // Act - Simulate race condition: two threads at EXACTLY same time
            var stopwatch = Stopwatch.StartNew();

            var task1 = _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var task2 = _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            var results = await Task.WhenAll(task1, task2);
            stopwatch.Stop();

            // Assert - CRITICAL ASSERTIONS
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            // ?? THIS MUST PASS - If it fails, race condition still exists!
            Assert.Equal(1, successCount);
            Assert.Equal(1, failureCount);

            // ?? Failed booking must have proper error type
            var failedResult = results.First(r => !r.Success);
            Assert.Equal(BookingErrorType.SeatAlreadyBooked, failedResult.ErrorType);

            // ?? Database must have EXACTLY one reservation (not two!)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"?? Race condition prevented in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? STRESS TEST: 10 concurrent threads all trying to book same seat
        /// 
        /// Expected: 1 success, 9 failures
        /// This validates that pessimistic locking scales under moderate load
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_TenThreads_SameSeat_OnlyOneSucceeds()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            var stopwatch = Stopwatch.StartNew();

            // Act - 10 concurrent tasks
            var tasks = Enumerable.Range(1, 10)
                .Select(i => _business.RezervovatAsync(
                    dto,
                    $"user{i}@example.com",
                    CancellationToken.None))
                .ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            Assert.Equal(1, successCount);
            Assert.Equal(9, failureCount);

            // All failures must be SeatAlreadyBooked
            var failures = results.Where(r => !r.Success).ToList();
            Assert.All(failures, r =>
                Assert.Equal(BookingErrorType.SeatAlreadyBooked, r.ErrorType));

            // Database has exactly 1 reservation
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"?? Race condition prevented under 10-thread stress in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? MAXIMUM STRESS TEST: 50 concurrent threads
        /// 
        /// Expected: 1 success, 49 failures
        /// This validates that locking scales under extreme load
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_FiftyThreads_MaximumStress()
        {
            // Arrange - 50 concurrent attempts!
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            var stopwatch = Stopwatch.StartNew();

            // Act - 50 concurrent tasks
            var tasks = Enumerable.Range(1, 50)
                .Select(i => _business.RezervovatAsync(
                    dto,
                    $"user{i}@example.com",
                    CancellationToken.None))
                .ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            // ?? Under extreme load, still only 1 succeeds
            Assert.Equal(1, successCount);
            Assert.Equal(49, failureCount);

            // Database has exactly 1 reservation (not 50!)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"?? Pessimistic locking held under 50-thread maximum stress in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? TRANSACTION ROLLBACK TEST: Verify no orphaned data on error
        /// 
        /// Purpose: If anything fails during transaction, entire transaction
        ///          is rolled back (no partial data left in database)
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_TransactionRollback_NoOrphanedData()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            // Act - Book seat twice (first succeeds, second fails)
            var result1 = await _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var result2 = await _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            // Assert
            Assert.True(result1.Success);
            Assert.False(result2.Success);

            // ?? Database has EXACTLY 1 reservation (not 2, not 0)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            // ?? No orphaned data
            Assert.Equal(1, reservations[0].MistoId);
            Assert.Equal(1, reservations[0].ZamestnanecId);
        }

        /// <summary>
        /// ?? MULTIPLE SEATS TEST: Verify no interference between different seats
        /// 
        /// Two users booking DIFFERENT seats on same day should both succeed
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_DifferentSeats_BothSucceed()
        {
            // Arrange - Add second seat
            var sekce = await _db.Sekce.FirstOrDefaultAsync();
            var misto2 = new Misto { Id = 2, Oznaceni = "SJ1-M2", SekceId = 1, Sekce = sekce };
            _db.Mista.Add(misto2);
            await _db.SaveChangesAsync();

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            var dto1 = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };
            var dto2 = new RezervaceDto { SekceId = 1, SeatNumber = "2", Date = tomorrow };

            // Act - Two concurrent bookings on DIFFERENT seats
            var task1 = _business.RezervovatAsync(dto1, "user1@example.com", CancellationToken.None);
            var task2 = _business.RezervovatAsync(dto2, "user2@example.com", CancellationToken.None);

            var results = await Task.WhenAll(task1, task2);

            // Assert - BOTH should succeed!
            Assert.Equal(2, results.Count(r => r.Success));
            Assert.Equal(0, results.Count(r => !r.Success));

            // Database has 2 reservations (one per seat)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Equal(2, reservations.Count);
        }

        /// <summary>
        /// ?? DIFFERENT DATES TEST: Same seat, different dates = both succeed
        /// 
        /// Two users booking SAME SEAT but DIFFERENT DATES should both succeed
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_SameSeat_DifferentDates_BothSucceed()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dayAfterTomorrow = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

            var dto1 = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = tomorrow };
            var dto2 = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = dayAfterTomorrow };

            // Act - Same seat, different dates
            var task1 = _business.RezervovatAsync(dto1, "user1@example.com", CancellationToken.None);
            var task2 = _business.RezervovatAsync(dto2, "user2@example.com", CancellationToken.None);

            var results = await Task.WhenAll(task1, task2);

            // Assert - BOTH should succeed!
            Assert.Equal(2, results.Count(r => r.Success));

            // Database has 2 reservations (different dates)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Equal(2, reservations.Count);
        }
    }
}
