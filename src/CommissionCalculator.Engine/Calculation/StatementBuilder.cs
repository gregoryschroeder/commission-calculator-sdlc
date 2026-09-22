using System.Globalization;

namespace CommissionCalculator.Engine.Calculation;

[Implements("FR-003")]
internal static class StatementBuilder
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static RepStatement Build(RepInput rep, QuarterPeriod quarter, IReadOnlyList<DealInput> deals)
    {
        var credits = QuarterCredit.For(rep.RepId, quarter, deals);
        var creditedBookings = credits.Sum(credit => credit.Share);
        var prorated = QuotaProration.For(rep.Quota, rep.StartDate, quarter);
        var proratedQuota = prorated.Amount;
        var tiers = TierSchedule.Lines(creditedBookings, proratedQuota);
        var commissionBeforeRefunds = tiers.Sum(tier => tier.Amount);

        List<BreakdownLine> lines = [QuotaLine(rep)];
        if (prorated.IsProrated)
        {
            lines.Add(new BreakdownLine(LineSection.Quota,
                $"Prorated quota ({prorated.DaysEmployed} of {prorated.DaysInQuarter} days)", proratedQuota, "FR-010"));
        }

        lines.AddRange(credits.Select(CreditLineFor));
        lines.Add(new BreakdownLine(LineSection.Subtotal, "Credited bookings", creditedBookings, "FR-008"));
        lines.AddRange(tiers.Select(TierLineFor));
        lines.Add(new BreakdownLine(LineSection.Subtotal, "Commission before refunds", commissionBeforeRefunds, "FR-006"));

        return new RepStatement(rep.RepId, rep.Name,
            Quota: rep.Quota, ProratedQuota: proratedQuota, CreditedBookings: creditedBookings,
            Attainment: Attainment(creditedBookings, proratedQuota),
            CommissionBeforeRefunds: commissionBeforeRefunds, Clawbacks: 0m, EarnedCommission: commissionBeforeRefunds,
            DrawPaid: 0m, Recovered: 0m, Payable: 0m, ClosingRecoverableBalance: 0m,
            lines);
    }

    [Implements("FR-005")]
    private static BreakdownLine QuotaLine(RepInput rep) =>
        new(LineSection.Quota, "Quarterly quota", rep.Quota, "FR-005");

    private static BreakdownLine CreditLineFor(DealCredit credit)
    {
        var booked = $"{credit.Deal.DealId} booked {Date(credit.Deal.BookingDate)} (closed {Date(credit.Deal.CloseDate)})";
        if (!credit.Counted)
        {
            return new BreakdownLine(LineSection.Excluded, $"{booked}: excluded, booked outside the quarter", 0m, "FR-008");
        }

        return credit.Split is null
            ? new BreakdownLine(LineSection.Credit, booked, credit.Share, "FR-008")
            : new BreakdownLine(LineSection.Credit,
                $"{booked}: {credit.Split.Percent.ToString("0.###", Invariant)}% share of {Dollars(credit.Deal.Amount)}",
                credit.Share, "FR-012");
    }

    private static BreakdownLine TierLineFor(TierLine tier) =>
        new(LineSection.Tier, $"{(tier.Rate * 100m).ToString("0.##", Invariant)}% of {Dollars(tier.Slice)}", tier.Amount, "FR-006");

    // Display only: attainment is never used to compute an amount (FR-009).
    [Implements("FR-009")]
    private static decimal Attainment(decimal creditedBookings, decimal proratedQuota) => creditedBookings / proratedQuota;

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", Invariant);

    private static string Dollars(decimal amount) => "$" + amount.ToString("#,##0.00", Invariant);
}
