using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class DrawRecoveryTests
{
    [Fact, Trait("Requirement", "FR-015")]
    public void CommissionAboveTheDrawIsPayable() =>
        Assert.Equal(new Recovery(12_000.00m, 3_000.00m, 0.00m), DrawRecovery.For(15_000.00m, 0.00m, 12_000.00m));

    [Fact, Trait("Requirement", "FR-015")]
    public void CommissionBelowTheDrawCarriesTheShortfallForward() =>
        Assert.Equal(new Recovery(4_000.00m, 0.00m, 8_000.00m), DrawRecovery.For(4_000.00m, 0.00m, 12_000.00m));

    [Fact, Trait("Requirement", "FR-015")]
    public void NoCommissionRecoversNothing() =>
        Assert.Equal(new Recovery(0.00m, 0.00m, 12_000.00m), DrawRecovery.For(0.00m, 0.00m, 12_000.00m));

    [Fact, Trait("Requirement", "FR-015")]
    public void AnOpeningBalanceIsRecoveredToo() =>
        Assert.Equal(new Recovery(15_000.00m, 0.00m, 5_000.00m), DrawRecovery.For(15_000.00m, 8_000.00m, 12_000.00m));

    [Fact, Trait("Requirement", "FR-015")]
    public void NegativeEarningsAreAddedToTheBalanceAndRecoverNothing() =>
        Assert.Equal(new Recovery(0.00m, 0.00m, 13_600.00m), DrawRecovery.For(-1_600.00m, 0.00m, 12_000.00m));

    [Theory, Trait("Requirement", "FR-015")]
    [InlineData("15000.00", "0.00", "12000.00")]
    [InlineData("4000.00", "0.00", "12000.00")]
    [InlineData("0.00", "0.00", "12000.00")]
    [InlineData("15000.00", "8000.00", "12000.00")]
    [InlineData("-1600.00", "0.00", "12000.00")]
    [InlineData("0.51", "0.00", "12000.00")]
    public void PayableIsNeverNegative(string earned, string opening, string draw)
    {
        decimal D(string value) => decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(DrawRecovery.For(D(earned), D(opening), D(draw)).Payable >= 0m);
    }
}
