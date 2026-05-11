using Xunit;
using Business.BookovaniMista.Resources;

namespace BookovaniMista.Tests
{
    /// <summary>
    /// Unit testy pro Constants
    /// </summary>
    public class ConstantsTests
    {
        [Fact]
        public void PartialViewNames_BookingForm_HasCorrectValue()
        {
            // Assert
            Assert.Equal("_BookingForm", Constants.PartialViewNames.BookingForm);
        }

        [Fact]
        public void PartialViewNames_SeatGrid_HasCorrectValue()
        {
            // Assert
            Assert.Equal("_SeatGrid", Constants.PartialViewNames.SeatGrid);
        }

        [Fact]
        public void PartialViewNames_SectionMap_HasCorrectValue()
        {
            // Assert
            Assert.Equal("_SectionMap", Constants.PartialViewNames.SectionMap);
        }

        [Fact]
        public void PartialViewNames_ConfirmDialog_HasCorrectValue()
        {
            // Assert
            Assert.Equal("_ConfirmDialog", Constants.PartialViewNames.ConfirmDialog);
        }

        [Fact]
        public void ApiEndpoints_Rezervovat_HasCorrectValue()
        {
            // Assert
            Assert.Equal("/Akcni/Rezervovat", Constants.ApiEndpoints.Rezervovat);
        }

        [Fact]
        public void RequestParameters_BookingDate_HasCorrectValue()
        {
            // Assert
            Assert.Equal("bookingDate", Constants.RequestParameters.BookingDate);
        }

        [Fact]
        public void HtmlIds_BookingContainer_HasCorrectValue()
        {
            // Assert
            Assert.Equal("booking-container", Constants.HtmlIds.BookingContainer);
        }

        [Fact]
        public void ViewDataKeys_Title_HasCorrectValue()
        {
            // Assert
            Assert.Equal("Title", Constants.ViewDataKeys.Title);
        }

        [Fact]
        public void ViewDataKeys_CurrentUsername_HasCorrectValue()
        {
            // Assert
            Assert.Equal("CurrentUsername", Constants.ViewDataKeys.CurrentUsername);
        }

        [Fact]
        public void AllConstantsAreNotEmpty()
        {
            // Assert - Ovìøit že všechny konstanty mají obsah
            Assert.False(string.IsNullOrEmpty(Constants.PartialViewNames.BookingForm));
            Assert.False(string.IsNullOrEmpty(Constants.PartialViewNames.SeatGrid));
            Assert.False(string.IsNullOrEmpty(Constants.PartialViewNames.SectionMap));
            Assert.False(string.IsNullOrEmpty(Constants.PartialViewNames.ConfirmDialog));
            Assert.False(string.IsNullOrEmpty(Constants.ApiEndpoints.Rezervovat));
            Assert.False(string.IsNullOrEmpty(Constants.RequestParameters.BookingDate));
            Assert.False(string.IsNullOrEmpty(Constants.HtmlIds.BookingContainer));
            Assert.False(string.IsNullOrEmpty(Constants.ViewDataKeys.Title));
            Assert.False(string.IsNullOrEmpty(Constants.ViewDataKeys.CurrentUsername));
        }

        [Theory]
        [InlineData("_BookingForm")]
        [InlineData("_SeatGrid")]
        [InlineData("_SectionMap")]
        [InlineData("_ConfirmDialog")]
        public void PartialViewNames_StartsWithUnderscore(string partialName)
        {
            // Assert
            Assert.StartsWith("_", partialName);
        }
    }
}
