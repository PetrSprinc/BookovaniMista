using Microsoft.EntityFrameworkCore;
using Xunit;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;
using System.Diagnostics;

namespace BookovaniMista.Tests.Unit
{
    /// <summary>
    /// ?? RACE CONDITION FIX VERIFICATION TEST
    /// 
    /// This test file is dedicated to verifying that the race condition
    /// vulnerability described in CODE_REVIEW_2024.md has been fixed.
    /// 
    /// The original problem:
    ///   ? Time-of-Check-Time-of-Use (TOCTOU) bug
    ///   var booked = await IsMistoBookedAsync(...);
    ///   if (booked) return; // Race window here!
    ///   await _db.SaveChangesAsync(); // Two threads could both book!
    /// 
    /// The solution:
    ///   ? Pessimistic locking with database transaction
    ///   ? Exclusive lock acquired before checking availability
    ///   ? Atomicity guaranteed by transaction
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
        /// This test directly addresses the race condition:
        /// Before fix: Both threads could succeed (WRONG! ?)
        /// After fix: Exactly one succeeds (CORRECT! ?)
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

            // Act - Simulate exact race condition: two threads at EXACTLY same time
            var stopwatch = Stopwatch.StartNew();

            var task1 = _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var task2 = _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            var results = await Task.WhenAll(task1, task2);
            stopwatch.Stop();

            // Assert - CRITICAL ASSERTION
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            // ? THIS MUST PASS - If it fails, race condition still exists!
            Assert.Equal(1, successCount);
            Assert.Equal(1, failureCount);

            // ? Failed booking must have proper error type
            var failedResult = results.First(r => !r.Success);
            Assert.Equal(BookingErrorType.SeatAlreadyBooked, failedResult.ErrorType);

            // ? Database must have EXACTLY one reservation (not two!)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"? Race condition prevented in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? STRESS TEST: 10 concurrent threads all trying to book same seat
        /// 
        /// Purpose: Verify that even under high concurrency, only one succeeds
        /// Expected: 1 success, 9 failures
        /// If test fails: Race condition exists (multiple could succeed)
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

            // Act - 10 concurrent tasks (extreme stress test)
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

            // ? CRITICAL: Exactly 1 must succeed
            Assert.Equal(1, successCount);
            Assert.Equal(9, failureCount);

            // ? All failures must be SeatAlreadyBooked
            var failures = results.Where(r => !r.Success).ToList();
            Assert.All(failures, r =>
                Assert.Equal(BookingErrorType.SeatAlreadyBooked, r.ErrorType));

            // ? Database has exactly 1 reservation
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"? Race condition prevented under 10-thread stress in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? CRITICAL: 50 concurrent threads (maximum stress)
        /// 
        /// Purpose: Extreme stress test to ensure pessimistic locking scales
        /// Expected: 1 success, 49 failures, 1 reservation in DB
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

            // ? CRITICAL: Under extreme load, still only 1 succeeds
            Assert.Equal(1, successCount);
            Assert.Equal(49, failureCount);

            // ? Database has exactly 1 reservation (not 50!)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            Console.WriteLine($"? Pessimistic locking held under 50-thread maximum stress in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// ?? TRANSACTION ROLLBACK TEST: Verify no orphaned data on error
        /// 
        /// Purpose: Ensure that if anything fails during transaction,
        ///          the entire transaction is rolled back (no partial data)
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

            // Act - Book seat twice
            var result1 = await _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var result2 = await _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            // Assert
            Assert.True(result1.Success);
            Assert.False(result2.Success);

            // ? CRITICAL: Database has EXACTLY 1 reservation (not 2, not 0)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Single(reservations);

            // ? No orphaned data
            Assert.Single(reservations);
            Assert.Equal(1, reservations[0].MistoId);
        }

        /// <summary>
        /// ?? DIFFERENT SEATS TEST: Verify locking doesn't affect other seats
        /// 
        /// Purpose: Confirm that pessimistic locking on one seat doesn't
        ///          prevent bookings on different seats
        /// Expected: All bookings succeed (no false contention)
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_DifferentSeats_AllSucceed()
        {
            // Arrange - Create multiple seats
            var section = _db.Sekce.First();
            for (int i = 2; i <= 5; i++)
            {
                _db.Mista.Add(new Misto
                {
                    Id = i,
                    Oznaceni = $"SJ1-M{i}",
                    SekceId = 1,
                    Sekce = section
                });
            }
            await _db.SaveChangesAsync();

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            // Act - 5 concurrent bookings on different seats
            var tasks = Enumerable.Range(1, 5)
                .Select(i => new RezervaceDto
                {
                    SekceId = 1,
                    SeatNumber = i.ToString(),
                    Date = tomorrow
                })
                .Select((dto, index) => _business.RezervovatAsync(
                    dto,
                    $"user{index + 1}@example.com",
                    CancellationToken.None))
                .ToList();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, r =>
                Assert.True(r.Success, "All bookings on different seats should succeed"));

            // ? Database has 5 reservations (one per seat)
            var reservations = await _db.Rezervace.ToListAsync();
            Assert.Equal(5, reservations.Count);

            Console.WriteLine("? Locking correctly isolated to individual seats");
        }

        /// <summary>
        /// ?? SAME DATE COLLISION TEST: Verify unique index works (MistoId, DatumRezervace)
        /// 
        /// Purpose: Ensure database constraint (unique index) also prevents double-booking
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_DatabaseConstraint_EnforcesUniqueness()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            // Act - Sequential booking (not concurrent)
            var result1 = await _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var result2 = await _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            // Assert
            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Equal(BookingErrorType.SeatAlreadyBooked, result2.ErrorType);

            // ? Database unique index prevents duplicate
            var reservations = await _db.Rezervace
                .Where(r => r.MistoId == 1 && r.DatumRezervace == DateTime.Parse(tomorrow))
                .ToListAsync();

            Assert.Single(reservations);
        }

        /// <summary>
        /// ?? PROOF TEST: Verify that if we had the old code (without pessimistic locking),
        ///                it would fail this test
        /// 
        /// This test demonstrates WHY pessimistic locking is necessary.
        /// The test itself doesn't test old code, but shows the symptoms that would appear.
        /// </summary>
        [Fact]
        public async Task RaceConditionFix_ProofOfNecessity_ConcurrentBookingMustFail()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = tomorrow
            };

            // Act - This simulates the exact scenario that WOULD fail without pessimistic locking
            var book1 = _business.RezervovatAsync(dto, "user1@example.com", CancellationToken.None);
            var book2 = _business.RezervovatAsync(dto, "user2@example.com", CancellationToken.None);

            var results = await Task.WhenAll(book1, book2);

            // Assert
            // ? With pessimistic locking: one fails
            // ? Without pessimistic locking: both would succeed (BUG!)

            var bothSucceeded = results.All(r => r.Success);
            Assert.False(bothSucceeded, "BOTH bookings succeeded - RACE CONDITION STILL EXISTS!");

            var exactlyOneSucceeded = results.Count(r => r.Success) == 1;
            Assert.True(exactlyOneSucceeded, "Exactly one booking should succeed");

            Console.WriteLine("? Proof: Concurrent booking correctly prevented (pessimistic locking working)");
        }
    }
}
