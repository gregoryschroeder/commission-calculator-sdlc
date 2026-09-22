namespace CommissionCalculator.Engine.Calculation;

internal sealed record ProratedQuota(decimal Amount, int DaysEmployed, int DaysInQuarter, bool IsProrated);

// A rep who starts after the quarter's first day is measured against a quota prorated by calendar
// days, start date to quarter end inclusive (clarification 2026-09-21, FR-010), rounded to the
// cent (FR-018) so every later step uses the rounded value.
[Implements("FR-010")]
internal static class QuotaProration
{
    public static ProratedQuota For(decimal quota, DateOnly startDate, QuarterPeriod quarter)
    {
        var daysInQuarter = Quarters.Days(quarter);
        if (startDate <= quarter.Start)
        {
            return new ProratedQuota(quota, daysInQuarter, daysInQuarter, IsProrated: false);
        }

        var daysEmployed = Math.Max(quarter.End.DayNumber - startDate.DayNumber + 1, 0);
        return new ProratedQuota(Money.Round(quota * daysEmployed / daysInQuarter), daysEmployed, daysInQuarter, IsProrated: true);
    }
}
