using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class DrawScheduleTests
{
    [Fact, Trait("Requirement", "FR-014")]
    public void AFullQuarterIsThreeWholeDraws() =>
        Assert.Equal(
            [new MonthDraw("January 2026", 4_000.00m), new MonthDraw("February 2026", 4_000.00m), new MonthDraw("March 2026", 4_000.00m)],
            DrawSchedule.For(ValidScenario.Past, ValidScenario.Q1));

    [Fact, Trait("Requirement", "FR-014")]
    public void TheStartMonthIsProratedByItsOwnDays() =>
        Assert.Equal(
            [new MonthDraw("January 2026", 0.00m), new MonthDraw("February 2026", 2_000.00m), new MonthDraw("March 2026", 4_000.00m)],
            DrawSchedule.For(new(2026, 2, 15), ValidScenario.Q1));

    [Fact, Trait("Requirement", "FR-018")]
    public void AProratedStartMonthIsRoundedToTheCent() =>
        Assert.Equal(
            [new MonthDraw("January 2026", 1_548.39m), new MonthDraw("February 2026", 4_000.00m), new MonthDraw("March 2026", 4_000.00m)],
            DrawSchedule.For(new(2026, 1, 20), ValidScenario.Q1));

    [Fact, Trait("Requirement", "FR-014")]
    public void StartingOnTheFirstOfAMonthIsAWholeDrawForThatMonth() =>
        Assert.Equal(
            [new MonthDraw("January 2026", 0.00m), new MonthDraw("February 2026", 4_000.00m), new MonthDraw("March 2026", 4_000.00m)],
            DrawSchedule.For(new(2026, 2, 1), ValidScenario.Q1));
}
