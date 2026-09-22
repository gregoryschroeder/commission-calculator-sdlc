using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class MoneyTests
{
    [Theory, Trait("Requirement", "FR-018")]
    [InlineData("0.505", "0.51")]
    [InlineData("-0.505", "-0.51")]
    [InlineData("1548.387096774193548387096774", "1548.39")]
    [InlineData("50549.450549450549450549450549", "50549.45")]
    public void RoundsToTheCentHalfAwayFromZero(string amount, string expected) =>
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            Money.Round(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));

    [Theory, Trait("Requirement", "FR-004")]
    [InlineData("10.10", true)]
    [InlineData("10.005", false)]
    public void RecognisesWholeCents(string amount, bool expected) =>
        Assert.Equal(expected, Money.IsWholeCents(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
}
