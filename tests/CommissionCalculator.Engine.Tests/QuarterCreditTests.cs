using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class QuarterCreditTests
{
    private static readonly DateOnly Closed = new(2026, 2, 10);

    private static DealCredit Credit(DateOnly booking, DateOnly? close = null)
    {
        var deal = ValidScenario.Deal("D", 1_000.00m, close ?? Closed, booking, [new SplitCredit("rep", 100m)]);
        return Assert.Single(QuarterCredit.For("rep", ValidScenario.Q1, [deal]));
    }

    [Fact, Trait("Requirement", "FR-008")]
    public void BookedTheDayBeforeTheQuarterIsExcluded()
    {
        var credit = Credit(new(2025, 12, 31));
        Assert.False(credit.Counted);
        Assert.Equal(0m, credit.Share);
    }

    [Fact, Trait("Requirement", "FR-008")]
    public void BookedTheDayAfterTheQuarterIsExcluded() => Assert.False(Credit(new(2026, 4, 1)).Counted);

    [Fact, Trait("Requirement", "FR-008")]
    public void BookedOnTheFirstDayCounts() => Assert.Equal(1_000.00m, Credit(new(2026, 1, 1)).Share);

    [Fact, Trait("Requirement", "FR-008")]
    public void BookedOnTheLastDayCounts() => Assert.Equal(1_000.00m, Credit(new(2026, 3, 31)).Share);

    [Fact, Trait("Requirement", "FR-008")]
    public void TheCloseDateNeverChangesTheResult()
    {
        Assert.True(Credit(new(2026, 2, 12), close: new(2025, 11, 1)).Counted);
        Assert.False(Credit(new(2026, 4, 2), close: new(2026, 3, 30)).Counted);
    }

    [Fact, Trait("Requirement", "FR-008")]
    public void DealsCreditedToOtherRepsAreNotListed()
    {
        var other = ValidScenario.Deal("O", 1_000.00m, Closed, new(2026, 2, 12), [new SplitCredit("other", 100m)]);
        Assert.Empty(QuarterCredit.For("rep", ValidScenario.Q1, [other]));
    }
}
