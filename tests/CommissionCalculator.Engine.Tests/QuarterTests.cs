using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class QuarterTests
{
    [Fact, Trait("Requirement", "FR-014")]
    public void FirstQuarterOf2026Has90DaysAndThreeMonths()
    {
        Assert.Equal(90, Quarters.Days(ValidScenario.Q1));
        Assert.Equal(
            [new CalendarMonth(new(2026, 1, 1), new(2026, 1, 31)),
             new CalendarMonth(new(2026, 2, 1), new(2026, 2, 28)),
             new CalendarMonth(new(2026, 3, 1), new(2026, 3, 31))],
            Quarters.Months(ValidScenario.Q1));
    }

    [Fact, Trait("Requirement", "FR-014")]
    public void SecondQuarterOf2026Has91Days() => Assert.Equal(91, Quarters.Days(ValidScenario.Q2));

    [Theory, Trait("Requirement", "FR-004")]
    [InlineData("2026-01-01", "2026-03-31", true)]
    [InlineData("2026-01-02", "2026-03-31", false)]
    [InlineData("2026-01-01", "2026-03-30", false)]
    [InlineData("2026-01-01", "2026-02-28", false)]
    [InlineData("2026-01-01", "2026-04-30", false)]
    public void OnlyThreeWholeCalendarMonthsFromTheFirstAreWellFormed(string start, string end, bool expected) =>
        Assert.Equal(expected, Quarters.IsWellFormed(new QuarterPeriod(DateOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture), DateOnly.Parse(end, System.Globalization.CultureInfo.InvariantCulture))));
}
