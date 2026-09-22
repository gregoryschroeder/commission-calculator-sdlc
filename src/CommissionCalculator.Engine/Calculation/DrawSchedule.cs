using System.Globalization;

namespace CommissionCalculator.Engine.Calculation;

internal sealed record MonthDraw(string Label, decimal Amount);

// $4,000.00 for each month of the quarter the rep is employed in; the month containing a
// mid-quarter start date is prorated by its own calendar days (clarification 2026-09-21, FR-014).
[Implements("FR-014")]
internal static class DrawSchedule
{
    private const decimal MonthlyDraw = 4_000.00m;

    public static IReadOnlyList<MonthDraw> For(DateOnly startDate, QuarterPeriod quarter) =>
        Quarters.Months(quarter).Select(month => new MonthDraw(Label(month), Amount(startDate, month))).ToList();

    private static decimal Amount(DateOnly startDate, CalendarMonth month)
    {
        if (startDate > month.Last)
        {
            return 0.00m;
        }

        if (startDate <= month.First)
        {
            return MonthlyDraw;
        }

        var daysInMonth = month.Last.DayNumber - month.First.DayNumber + 1;
        var daysEmployed = month.Last.DayNumber - startDate.DayNumber + 1;
        return Money.Round(MonthlyDraw * daysEmployed / daysInMonth);
    }

    private static string Label(CalendarMonth month) => month.First.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
}
