using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class TierScheduleTests
{
    [Fact, Trait("Requirement", "FR-006")]
    public void EightyPercentIsOneFivePercentLine() =>
        Assert.Equal([new TierLine(0.05m, 80_000.00m, 4_000.00m)], TierSchedule.Lines(80_000.00m, 100_000.00m));

    [Fact, Trait("Requirement", "FR-006")]
    public void EachBandEarnsItsOwnRateOnItsOwnSlice() =>
        Assert.Equal(
            [new TierLine(0.05m, 100_000.00m, 5_000.00m), new TierLine(0.08m, 50_000.00m, 4_000.00m), new TierLine(0.12m, 10_000.00m, 1_200.00m)],
            TierSchedule.Lines(160_000.00m, 100_000.00m));

    [Fact, Trait("Requirement", "FR-007")]
    public void ExactlyOneHundredFiftyPercentHasNoTwelvePercentLine() =>
        Assert.Equal(
            [new TierLine(0.05m, 100_000.00m, 5_000.00m), new TierLine(0.08m, 50_000.00m, 4_000.00m)],
            TierSchedule.Lines(150_000.00m, 100_000.00m));

    [Fact, Trait("Requirement", "FR-007")]
    public void ExactlyOneHundredPercentHasNoEightPercentLine() =>
        Assert.Equal([new TierLine(0.05m, 100_000.00m, 5_000.00m)], TierSchedule.Lines(100_000.00m, 100_000.00m));

    [Fact, Trait("Requirement", "FR-018")]
    public void EachLineIsRoundedHalfAwayFromZero() =>
        Assert.Equal([new TierLine(0.05m, 10.10m, 0.51m)], TierSchedule.Lines(10.10m, 100_000.00m));

    [Fact, Trait("Requirement", "FR-018")]
    public void TheOneHundredFiftyPercentEdgeIsRoundedFromTheRoundedQuota()
    {
        // 1.5 × 50,549.45 = 75,824.175 → 75,824.18, so the 8% slice is 25,274.73.
        var lines = TierSchedule.Lines(80_000.00m, 50_549.45m);

        Assert.Equal(
            [new TierLine(0.05m, 50_549.45m, 2_527.47m), new TierLine(0.08m, 25_274.73m, 2_021.98m), new TierLine(0.12m, 4_175.82m, 501.10m)],
            lines);
    }

    [Fact, Trait("Requirement", "FR-006")]
    public void NoCreditHasNoLines() => Assert.Empty(TierSchedule.Lines(0m, 100_000.00m));
}
