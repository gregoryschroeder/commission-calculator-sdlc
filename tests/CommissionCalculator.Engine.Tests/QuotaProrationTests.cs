using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class QuotaProrationTests
{
    [Fact, Trait("Requirement", "FR-010")]
    public void FortyFiveOfNinetyDaysHalvesTheQuota()
    {
        var prorated = QuotaProration.For(90_000.00m, new(2026, 2, 15), ValidScenario.Q1);

        Assert.Equal(new ProratedQuota(45_000.00m, 45, 90, IsProrated: true), prorated);
    }

    [Fact, Trait("Requirement", "FR-018")]
    public void FortySixOfNinetyOneDaysIsRoundedToTheCent()
    {
        var prorated = QuotaProration.For(100_000.00m, new(2026, 5, 16), ValidScenario.Q2);

        Assert.Equal(new ProratedQuota(50_549.45m, 46, 91, IsProrated: true), prorated);
    }

    [Theory, Trait("Requirement", "FR-010")]
    [InlineData("2026-01-01")]
    [InlineData("2025-06-01")]
    public void StartingOnOrBeforeTheFirstDayIsNotProrated(string start)
    {
        var prorated = QuotaProration.For(90_000.00m, DateOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture), ValidScenario.Q1);

        Assert.Equal(new ProratedQuota(90_000.00m, 90, 90, IsProrated: false), prorated);
    }

    [Fact, Trait("Requirement", "FR-010")]
    public void StartingOnTheLastDayIsOneDay()
    {
        var prorated = QuotaProration.For(90_000.00m, new(2026, 3, 31), ValidScenario.Q1);

        Assert.Equal(new ProratedQuota(1_000.00m, 1, 90, IsProrated: true), prorated);
    }
}
