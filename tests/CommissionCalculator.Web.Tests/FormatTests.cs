using CommissionCalculator.Web.Presentation;
using Xunit;

namespace CommissionCalculator.Web.Tests;

public sealed class FormatTests
{
    [Theory, Trait("Requirement", "FR-002")]
    [InlineData("0.8", "80.00%")]
    [InlineData("0.12345", "12.35%")]
    [InlineData("1.6", "160.00%")]
    public void AttainmentIsAPercentageToTwoDecimalsHalfAwayFromZero(string ratio, string expected) =>
        Assert.Equal(expected, Format.Attainment(decimal.Parse(ratio, System.Globalization.CultureInfo.InvariantCulture)));

    [Theory, Trait("Requirement", "FR-021"), Trait("Requirement", "FR-018")]
    [InlineData("1234.5", "$1,234.50")]
    [InlineData("0", "$0.00")]
    [InlineData("-1600", "−$1,600.00")]
    public void MoneyIsDollarsWithAMinusSignInTheText(string amount, string expected) =>
        Assert.Equal(expected, Format.Money(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
}
