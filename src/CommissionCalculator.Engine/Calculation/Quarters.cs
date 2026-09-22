namespace CommissionCalculator.Engine.Calculation;

internal sealed record CalendarMonth(DateOnly First, DateOnly Last);

internal static class Quarters
{
    private const int MonthsPerQuarter = 3;

    public static int Days(QuarterPeriod quarter) => quarter.End.DayNumber - quarter.Start.DayNumber + 1;

    [Implements("FR-014")]
    public static IReadOnlyList<CalendarMonth> Months(QuarterPeriod quarter) =>
        Enumerable.Range(0, MonthsPerQuarter)
            .Select(offset => quarter.Start.AddMonths(offset))
            .Select(first => new CalendarMonth(first, first.AddMonths(1).AddDays(-1)))
            .ToList();

    [Implements("FR-004")]
    public static bool IsWellFormed(QuarterPeriod quarter) =>
        quarter.Start.Day == 1 && quarter.End == quarter.Start.AddMonths(MonthsPerQuarter).AddDays(-1);
}
