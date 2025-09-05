using Xunit;
using CarDiagnostics.Domain.Utils;

namespace CarDiagnostics.Tests
{
    public class PlateValidatorTests
    {
        [Theory]
        [InlineData("1234567")]
        [InlineData("123-45-678")]
        [InlineData("  12-3 45 678  ")]
        [InlineData("0012345")]
        [InlineData("12-345-67")]   // ✅ 7 ספרות בפורמט ישראלי ותיק
        [InlineData("123-45-678")]  // ✅ 8 ספרות בפורמט הישראלי הנוכחי
        public void IsValid_ReturnsTrue_ForSevenOrEightDigitsAfterNormalize(string input)
        {
            Assert.True(PlateValidator.IsValid(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123456")]      // 6 ספרות
        [InlineData("123456789")]   // 9 ספרות
        [InlineData("abc")]         // 0 ספרות
        public void IsValid_ReturnsFalse_ForInvalidPlates(string? input)
        {
            Assert.False(PlateValidator.IsValid(input));
        }

        [Theory]
        [InlineData(" 12-3 45 678 ", "12345678")]
        [InlineData("123-4567",      "1234567")]
        [InlineData(null,            "")]
        public void Normalize_StripsNonDigits(string? input, string expected)
        {
            Assert.Equal(expected, PlateValidator.Normalize(input));
        }
    }
}
