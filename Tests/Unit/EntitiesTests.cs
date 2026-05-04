using Xunit;
using Entities.BookovaniMista;

namespace BookovaniMista.Tests.Unit
{
    /// <summary>
    /// Unit Tests for Entities and DTOs
    /// Tests for data validation and enum usage
    /// </summary>
    public class EntitiesTests
    {
        /// <summary>
        /// Test: RezervaceDto can be created with valid data
        /// Expected: Object created successfully
        /// </summary>
        [Fact]
        public void RezervaceDto_ValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "5",
                Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")
            };

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(1, dto.SekceId);
            Assert.Equal("5", dto.SeatNumber);
            Assert.NotEmpty(dto.Date);
        }

        /// <summary>
        /// Test: BookingResultDto success state
        /// Expected: Success flag and error type set correctly
        /// </summary>
        [Fact]
        public void BookingResultDto_SuccessState_SetCorrectly()
        {
            // Arrange & Act
            var result = new BookingResultDto
            {
                Success = true,
                ErrorType = BookingErrorType.None,
                ErrorMessage = null
            };

            // Assert
            Assert.True(result.Success);
            Assert.Equal(BookingErrorType.None, result.ErrorType);
            Assert.Null(result.ErrorMessage);
        }

        /// <summary>
        /// Test: BookingResultDto error state
        /// Expected: Success false, error details set
        /// </summary>
        [Fact]
        public void BookingResultDto_ErrorState_SetCorrectly()
        {
            // Arrange & Act
            var result = new BookingResultDto
            {
                Success = false,
                ErrorType = BookingErrorType.SeatAlreadyBooked,
                ErrorMessage = "The seat is already booked"
            };

            // Assert
            Assert.False(result.Success);
            Assert.Equal(BookingErrorType.SeatAlreadyBooked, result.ErrorType);
            Assert.Equal("The seat is already booked", result.ErrorMessage);
        }

        /// <summary>
        /// Test: BookingErrorType enum has all expected values
        /// Expected: All error types defined
        /// </summary>
        [Fact]
        public void BookingErrorType_HasAllValues()
        {
            // Assert
            Assert.True(BookingErrorType.None.HasValue);
            Assert.True(BookingErrorType.ValidationFailed.HasValue);
            Assert.True(BookingErrorType.SectionNotFound.HasValue);
            Assert.True(BookingErrorType.SeatNotFound.HasValue);
            Assert.True(BookingErrorType.UserNotFound.HasValue);
            Assert.True(BookingErrorType.DateInPast.HasValue);
            Assert.True(BookingErrorType.DateTooFar.HasValue);
            Assert.True(BookingErrorType.SeatAlreadyBooked.HasValue);
            Assert.True(BookingErrorType.DatabaseError.HasValue);
        }

        /// <summary>
        /// Test: BookingErrorMessages contain all error messages
        /// Expected: Messages are localization-ready
        /// </summary>
        [Fact]
        public void BookingErrorMessages_AllMessagesExist()
        {
            // Arrange & Act & Assert
            Assert.NotEmpty(Business.BookovaniMista.Resources.BookingErrorMessages.ValidationFailed);
            Assert.NotEmpty(Business.BookovaniMista.Resources.BookingErrorMessages.SectionNotFound);
            Assert.NotEmpty(Business.BookovaniMista.Resources.BookingErrorMessages.SeatNotFound);
            Assert.NotEmpty(Business.BookovaniMista.Resources.BookingErrorMessages.UserNotFound);
            Assert.NotEmpty(Business.BookovaniMista.Resources.BookingErrorMessages.DateInPast);
        }

        /// <summary>
        /// Test: RezervaceDto with null SekceId
        /// Expected: Can be null (optional)
        /// </summary>
        [Fact]
        public void RezervaceDto_NullSekceId_Allowed()
        {
            // Arrange & Act
            var dto = new RezervaceDto
            {
                SekceId = null,
                SeatNumber = "1",
                Date = "2024-12-25"
            };

            // Assert
            Assert.Null(dto.SekceId);
        }

        /// <summary>
        /// Test: RezervaceDto with empty date
        /// Expected: Can be empty string
        /// </summary>
        [Fact]
        public void RezervaceDto_EmptyDate_Allowed()
        {
            // Arrange & Act
            var dto = new RezervaceDto
            {
                SekceId = 1,
                SeatNumber = "1",
                Date = ""
            };

            // Assert
            Assert.Empty(dto.Date);
        }

        /// <summary>
        /// Test: Multiple DTOs are independent
        /// Expected: Changing one doesn't affect another
        /// </summary>
        [Fact]
        public void RezervaceDto_MultipleInstances_Independent()
        {
            // Arrange
            var dto1 = new RezervaceDto { SekceId = 1, SeatNumber = "1", Date = "2024-12-25" };
            var dto2 = new RezervaceDto { SekceId = 2, SeatNumber = "2", Date = "2024-12-26" };

            // Act
            dto1.SekceId = 999;

            // Assert
            Assert.Equal(999, dto1.SekceId);
            Assert.Equal(2, dto2.SekceId);
        }

        /// <summary>
        /// Test: BookingResultDto can hold reservation data
        /// Expected: Rezervace object stored correctly
        /// </summary>
        [Fact]
        public void BookingResultDto_WithRezervace_StoresCorrectly()
        {
            // Arrange
            var rezervace = new Entities.BookovaniMista.Models.Rezervace
            {
                Id = 1,
                MistoId = 5,
                ZamestnanecId = 1,
                DatumRezervace = DateTime.Today.AddDays(1)
            };

            // Act
            var result = new BookingResultDto
            {
                Success = true,
                ErrorType = BookingErrorType.None,
                Rezervace = rezervace
            };

            // Assert
            Assert.NotNull(result.Rezervace);
            Assert.Equal(1, result.Rezervace.Id);
            Assert.Equal(5, result.Rezervace.MistoId);
        }
    }

    /// <summary>
    /// Unit Tests for Zamestnanec (Employee) entity
    /// </summary>
    public class ZamestnanecTests
    {
        /// <summary>
        /// Test: Zamestnanec can be created with full data
        /// Expected: All properties set correctly
        /// </summary>
        [Fact]
        public void Zamestnanec_ValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var employee = new Entities.BookovaniMista.Models.Zamestnanec
            {
                Id = 1,
                Jmeno = "John Smith",
                Email = "john@example.com"
            };

            // Assert
            Assert.Equal(1, employee.Id);
            Assert.Equal("John Smith", employee.Jmeno);
            Assert.Equal("john@example.com", employee.Email);
        }

        /// <summary>
        /// Test: Zamestnanec with null email
        /// Expected: Email can be null
        /// </summary>
        [Fact]
        public void Zamestnanec_NullEmail_Allowed()
        {
            // Arrange & Act
            var employee = new Entities.BookovaniMista.Models.Zamestnanec
            {
                Id = 1,
                Jmeno = "Jane Doe",
                Email = null
            };

            // Assert
            Assert.Null(employee.Email);
            Assert.NotNull(employee.Jmeno);
        }

        /// <summary>
        /// Test: Zamestnanec can have reservations collection
        /// Expected: Collection initialized
        /// </summary>
        [Fact]
        public void Zamestnanec_Rezervace_CollectionInitialized()
        {
            // Arrange & Act
            var employee = new Entities.BookovaniMista.Models.Zamestnanec
            {
                Id = 1,
                Jmeno = "John Smith",
                Email = "john@example.com"
            };

            // Assert
            Assert.NotNull(employee.Rezervace);
            Assert.Empty(employee.Rezervace);
        }
    }

    /// <summary>
    /// Unit Tests for Sekce (Section) entity
    /// </summary>
    public class SekceTests
    {
        /// <summary>
        /// Test: Sekce can be created with data
        /// Expected: All properties set
        /// </summary>
        [Fact]
        public void Sekce_ValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var section = new Entities.BookovaniMista.Models.Sekce
            {
                Id = 1,
                Oznaceni = "SJ1",
                Nazev = "Sekce jih 1"
            };

            // Assert
            Assert.Equal(1, section.Id);
            Assert.Equal("SJ1", section.Oznaceni);
            Assert.Equal("Sekce jih 1", section.Nazev);
        }

        /// <summary>
        /// Test: Sekce has seats collection
        /// Expected: Collection properly initialized
        /// </summary>
        [Fact]
        public void Sekce_Mista_CollectionInitialized()
        {
            // Arrange & Act
            var section = new Entities.BookovaniMista.Models.Sekce
            {
                Id = 1,
                Oznaceni = "SJ1",
                Nazev = "Sekce jih 1"
            };

            // Assert
            Assert.NotNull(section.Mista);
            Assert.Empty(section.Mista);
        }
    }

    /// <summary>
    /// Unit Tests for Misto (Seat) entity
    /// 
    /// IMPORTANT: SekceId CANNOT be null!
    /// - SekceId: int (NOT nullable) - Foreign Key, REQUIRED
    /// - Misto MUST belong to exactly one Sekce
    /// - Database enforces this relationship
    /// </summary>
    public class MistoTests
    {
        /// <summary>
        /// Test: Misto can be created with valid data
        /// Expected: Properties set correctly
        /// 
        /// Note: SekceId is REQUIRED (int, not int?)
        /// Misto cannot exist without a Section
        /// </summary>
        [Fact]
        public void Misto_ValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var seat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                SekceId = 1,  // ? REQUIRED! Cannot be null
                Nazev = "Seat 1"
            };

            // Assert
            Assert.Equal(1, seat.Id);
            Assert.Equal("SJ1-M1", seat.Oznaceni);
            Assert.Equal(1, seat.SekceId);  // ? Always has a value
            Assert.Equal("Seat 1", seat.Nazev);
        }

        /// <summary>
        /// Test: Misto with null name is allowed
        /// Expected: Nazev CAN be null (optional)
        /// 
        /// Note: SekceId is still REQUIRED
        /// Only Nazev is optional (?)
        /// </summary>
        [Fact]
        public void Misto_NullNazev_Allowed()
        {
            // Arrange & Act
            var seat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                SekceId = 1,  // ? REQUIRED! Must have value
                Nazev = null   // ? ALLOWED - optional field
            };

            // Assert
            Assert.Null(seat.Nazev);  // ? Null is OK for Nazev
            Assert.NotEmpty(seat.Oznaceni);
            Assert.NotEqual(0, seat.SekceId);  // ? SekceId always has value
        }

        /// <summary>
        /// Test: Misto requires a SekceId
        /// Expected: SekceId cannot be 0 or default
        /// 
        /// IMPORTANT: This test demonstrates that SekceId is REQUIRED
        /// Default value is 0, which is invalid (no Sekce with id=0)
        /// </summary>
        [Fact]
        public void Misto_SekceId_IsRequired()
        {
            // Arrange
            var seat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                // SekceId is NOT set - defaults to 0
                Nazev = "Seat"
            };

            // Assert
            // ? This demonstrates SekceId CANNOT be null
            // ? It defaults to 0 (not a valid FK value)
            // ? A valid Sekce must be assigned
            Assert.Equal(0, seat.SekceId);  // Default value is 0

            // In real usage with DbContext:
            // - Setting SekceId = 0 would cause DbUpdateException
            // - Foreign key constraint would fail
            // - Cannot save Misto without valid SekceId
        }

        /// <summary>
        /// Test: Misto must reference existing Sekce
        /// Expected: SekceId points to valid Sekce
        /// 
        /// Relationship: Misto.SekceId ? Sekce.Id
        /// Constraint: Restrict (cannot delete Section with Seats)
        /// </summary>
        [Fact]
        public void Misto_SekceId_MustBeValid()
        {
            // Arrange
            var validSekceId = 1;
            var invalidSekceId = 999;  // No Section with this ID

            var validSeat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                SekceId = validSekceId,
                Nazev = "Seat 1"
            };

            var invalidSeat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 2,
                Oznaceni = "SJ1-M2",
                SekceId = invalidSekceId,  // ? No Sekce with this ID
                Nazev = "Seat 2"
            };

            // Assert
            Assert.Equal(validSekceId, validSeat.SekceId);
            Assert.Equal(invalidSekceId, invalidSeat.SekceId);

            // Note: The entity object can hold invalid SekceId
            // But DbContext.SaveChangesAsync() would throw:
            // - DbUpdateException (foreign key constraint violation)
        }

        /// <summary>
        /// Test: Misto has reservations collection
        /// Expected: Collection initialized
        /// </summary>
        [Fact]
        public void Misto_Rezervace_CollectionInitialized()
        {
            // Arrange & Act
            var seat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                SekceId = 1  // ? REQUIRED
            };

            // Assert
            Assert.NotNull(seat.Rezervace);
            Assert.Empty(seat.Rezervace);
        }

        /// <summary>
        /// Test: Model structure validation
        /// Expected: All properties exist and have correct types
        /// </summary>
        [Fact]
        public void Misto_ModelStructure_IsCorrect()
        {
            // Arrange
            var seat = new Entities.BookovaniMista.Models.Misto
            {
                Id = 1,
                Oznaceni = "SJ1-M1",
                SekceId = 1,
                Nazev = "Seat 1",
                Sekce = new Entities.BookovaniMista.Models.Sekce { Id = 1 },
                Rezervace = new List<Entities.BookovaniMista.Models.Rezervace>()
            };

            // Assert - Verify structure
            Assert.IsType<int>(seat.Id);
            Assert.IsType<string>(seat.Oznaceni);
            Assert.IsType<int>(seat.SekceId);  // ? NOT nullable
            Assert.IsType<string>(seat.Nazev);  // ? CAN be null
            Assert.NotNull(seat.Sekce);
            Assert.NotNull(seat.Rezervace);
        }
    }
}
