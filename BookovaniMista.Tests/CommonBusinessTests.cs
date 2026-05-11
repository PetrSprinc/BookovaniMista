using Xunit;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;
using System.Security.Claims;

namespace BookovaniMista.Tests
{
    /// <summary>
    /// Unit testy pro CommonBusiness tøídu
    /// </summary>
    public class CommonBusinessTests : IDisposable
    {
        private readonly BookovaniMistaDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly CommonBusiness _commonBusiness;

        public CommonBusinessTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<BookovaniMistaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new BookovaniMistaDbContext(options);
            _cache = new MemoryCache(new MemoryCacheOptions());
            _commonBusiness = new CommonBusiness(_dbContext, _cache);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _cache?.Dispose();
        }

        #region GetSectionDefinitions Tests

        [Fact]
        public void GetSectionDefinitions_ReturnsSectionInfo()
        {
            // Act
            var sections = _commonBusiness.GetSectionDefinitions();

            // Assert
            Assert.NotNull(sections);
            Assert.NotEmpty(sections);
            Assert.Equal(9, sections.Length);
        }

        [Fact]
        public void GetSectionDefinitions_ReturnsCorrectSectionData()
        {
            // Act
            var sections = _commonBusiness.GetSectionDefinitions();

            // Assert
            var firstSection = sections.First();
            Assert.Equal("SJ1", firstSection.Id);
            Assert.Equal(1, firstSection.Db);
            Assert.Equal("Sekce jih 1", firstSection.Title);
        }

        [Fact]
        public void GetSectionDefinitions_CacheWorks_SecondCallReturnsCachedData()
        {
            // Act
            var sections1 = _commonBusiness.GetSectionDefinitions();
            var sections2 = _commonBusiness.GetSectionDefinitions();

            // Assert
            Assert.Same(sections1, sections2); // Stejný reference = z cache
        }

        [Fact]
        public void GetSectionDefinitions_AllSectionsHaveRequiredProperties()
        {
            // Act
            var sections = _commonBusiness.GetSectionDefinitions();

            // Assert
            foreach (var section in sections)
            {
                Assert.NotNull(section.Id);
                Assert.True(section.Db > 0);
                Assert.NotNull(section.Title);
                Assert.NotNull(section.Subtitle);
                Assert.True(section.Total > 0);
                Assert.True(section.Rows > 0);
            }
        }

        #endregion

        #region ParseBookingDate Tests

        [Fact]
        public void ParseBookingDate_ValidDate_ReturnsParsedDate()
        {
            // Act
            var result = CommonBusiness.ParseBookingDate("2024-01-15");

            // Assert
            Assert.Equal(new DateTime(2024, 1, 15), result);
        }

        [Fact]
        public void ParseBookingDate_InvalidDate_ReturnsToday()
        {
            // Act
            var result = CommonBusiness.ParseBookingDate("invalid");

            // Assert
            Assert.Equal(DateTime.Today, result);
        }

        [Fact]
        public void ParseBookingDate_NullString_ReturnsToday()
        {
            // Act
            var result = CommonBusiness.ParseBookingDate(null);

            // Assert
            Assert.Equal(DateTime.Today, result);
        }

        [Fact]
        public void ParseBookingDate_EmptyString_ReturnsToday()
        {
            // Act
            var result = CommonBusiness.ParseBookingDate("");

            // Assert
            Assert.Equal(DateTime.Today, result);
        }

        [Fact]
        public void ParseBookingDate_ValidDateWithTime_ReturnsMidnight()
        {
            // Act
            var result = CommonBusiness.ParseBookingDate("2024-01-15 14:30:00");

            // Assert
            Assert.Equal(new DateTime(2024, 1, 15, 0, 0, 0), result);
        }

        #endregion

        #region GetCurrentZamestnanecAsync Tests

        [Fact]
        public async Task GetCurrentZamestnanecAsync_NullUser_ReturnsNull()
        {
            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(null!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentZamestnanecAsync_EmailNotFound_ReturnsNull()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "nonexistent@example.com")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(principal);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentZamestnanecAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var testUser = new Zamestnanec
            {
                Id = 1,
                Jmeno = "Jan Novák",
                Email = "jan@example.com"
            };
            _dbContext.Zamestnanci.Add(testUser);
            await _dbContext.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "jan@example.com")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jan Novák", result.Jmeno);
            Assert.Equal("jan@example.com", result.Email);
        }

        #endregion
    }
}
