using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

// Largest remainder (clarification 2026-09-21): shares are rounded down, then the leftover cents
// go to the largest fractional remainders, ties to the rep listed first.
public sealed class SplitAllocationTests
{
    private static IReadOnlyList<decimal> Allocate(decimal amount, params decimal[] percents) =>
        SplitAllocation.Allocate(amount, percents.Select((percent, index) => new SplitCredit($"rep{index}", percent)).ToList());

    [Fact, Trait("Requirement", "FR-012")]
    public void SharesThatDivideEvenlyAreExact() =>
        Assert.Equal([30_000.00m, 20_000.00m], Allocate(50_000.00m, 60m, 40m));

    [Fact, Trait("Requirement", "FR-012")]
    public void TheLeftoverCentGoesToTheFirstListedRepOnATie() =>
        Assert.Equal([5.01m, 5.00m], Allocate(10.01m, 50m, 50m));

    [Fact, Trait("Requirement", "FR-012")]
    public void TheLargestRemainderTakesTheLeftoverCent() =>
        Assert.Equal([33.34m, 33.33m, 33.33m], Allocate(100.00m, 33.335m, 33.335m, 33.33m));

    [Fact, Trait("Requirement", "FR-012")]
    public void ASubCentShareCanRoundToNothing() =>
        Assert.Equal([0.03m, 0.03m, 0.00m], Allocate(0.06m, 45m, 45m, 10m));

    [Fact, Trait("Requirement", "FR-017")]
    public void ReSplittingAReducedAmountCanMoveTheCent() =>
        Assert.Equal([0.02m, 0.02m, 0.01m], Allocate(0.05m, 45m, 45m, 10m));

    [Theory, Trait("Requirement", "FR-012")]
    [InlineData("0.01", 50.0, 50.0)]
    [InlineData("0.03", 45.0, 45.0, 10.0)]
    [InlineData("10000.20", 50.0, 50.0)]
    [InlineData("100.00", 33.335, 33.335, 33.33)]
    [InlineData("99999.99", 60.0, 40.0)]
    [InlineData("1.00", 25.0, 25.0, 25.0, 25.0)]
    public void SharesAlwaysSumToTheAmount(string amount, params double[] percents)
    {
        var value = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(value, Allocate(value, percents.Select(percent => (decimal)percent).ToArray()).Sum());
    }
}
