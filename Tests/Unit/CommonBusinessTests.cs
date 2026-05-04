using Microsoft.EntityFrameworkCore;
using Xunit;
using Entities.BookovaniMista;
using Entities.BookovaniMista.Models;
using Business.BookovaniMista;

namespace BookovaniMista.Tests.Unit
{
    /// <summary>
    /// Unit Tests for CommonBusiness class
    /// Tests for user identification, employee lookup, and common operations
    /// </summary>
    public class CommonBusinessTests : IAsyncLifetime
    {
        private BookovaniMistaDbContext _db;
        private CommonBusiness _commonBusiness;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<BookovaniMistaDbContext>()
                .UseInMemoryDatabase($"CommonBusinessTest_{Guid.NewGuid()}")
                .Options;

            _db = new BookovaniMistaDbContext(options);
            _commonBusiness = new CommonBusiness(_db);
            await SeedTestData();
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        private async Task SeedTestData()
        {
            var zamestnanci = new List<Zamestnanec>
            {
                new Zamestnanec { Id = 1, Jmeno = "John Smith", Email = "john.smith@example.com" },
                new Zamestnanec { Id = 2, Jmeno = "Jane Doe", Email = "jane.doe@example.com" },
                new Zamestnanec { Id = 3, Jmeno = "Bob Johnson", Email = "bob.johnson@example.com" },
                new Zamestnanec { Id = 4, Jmeno = "Alice Brown", Email = null }
            };

            _db.Zamestnanci.AddRange(zamestnanci);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Test: Find employee by email claim
        /// Expected: Employee found correctly
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_ByEmail_FindsEmployee()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "john.smith@example.com")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.Jmeno);
            Assert.Equal("john.smith@example.com", result.Email);
        }

        /// <summary>
        /// Test: Email lookup is case-insensitive
        /// Expected: Employee found despite different case
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_EmailCaseInsensitive_FindsEmployee()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "JOHN.SMITH@EXAMPLE.COM")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.Jmeno);
        }

        /// <summary>
        /// Test: Find employee by identity name
        /// Expected: Employee found by full name
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_ByIdentityName_FindsEmployee()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>();
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Basic", "name", "role");
            identity.AddClaim(new System.Security.Claims.Claim("name", "John Smith"));
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.Jmeno);
        }

        /// <summary>
        /// Test: Find employee by domain username (Windows auth)
        /// Expected: Employee found by short name after backslash
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_ByDomainUsername_FindsEmployee()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>();
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Windows");
            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "DOMAIN\\John"));
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.Jmeno);
        }

        /// <summary>
        /// Test: Null user returns null
        /// Expected: No exception, returns null
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_NullUser_ReturnsNull()
        {
            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Unknown email returns null
        /// Expected: No employee found
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_UnknownEmail_ReturnsNull()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "unknown@example.com")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Employee with null email
        /// Expected: Cannot find by email, other methods work
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_EmployeeWithNullEmail_FindsByName()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>();
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Basic", "name", "role");
            identity.AddClaim(new System.Security.Claims.Claim("name", "Alice Brown"));
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice Brown", result.Jmeno);
            Assert.Null(result.Email);
        }

        /// <summary>
        /// Test: Whitespace in claims is trimmed
        /// Expected: Email found after trimming
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_EmailWithWhitespace_FindsEmployee()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "  john.smith@example.com  ")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Smith", result.Jmeno);
        }

        /// <summary>
        /// Test: Multiple claim types - email takes precedence
        /// Expected: Email claim is used first
        /// </summary>
        [Fact]
        public async Task GetCurrentZamestnanecAsync_MultipleClaims_EmailTakesPrecedence()
        {
            // Arrange
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "john.smith@example.com"),
                new System.Security.Claims.Claim("upn", "jane.doe@example.com")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var user = new System.Security.Claims.ClaimsPrincipal(identity);

            // Act
            var result = await _commonBusiness.GetCurrentZamestnanecAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id); // John Smith has ID 1
            Assert.Equal("John Smith", result.Jmeno);
        }
    }
}
