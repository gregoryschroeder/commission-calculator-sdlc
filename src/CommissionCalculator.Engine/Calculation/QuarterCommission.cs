namespace CommissionCalculator.Engine.Calculation;

// One rep's commission for one quarter, with a given set of refunds already applied. Clawbacks
// are the difference between two of these (FR-016).
internal sealed record QuarterContext(string RepId, decimal Quota, DateOnly StartDate, QuarterPeriod Quarter, IReadOnlyList<DealInput> Deals);

internal static class QuarterCommission
{
    public static decimal For(QuarterContext context, IReadOnlySet<RefundKey> appliedRefunds)
    {
        var credited = QuarterCredit.For(context.RepId, context.Quarter, context.Deals, appliedRefunds)
            .Sum(credit => credit.Share);
        var proratedQuota = QuotaProration.For(context.Quota, context.StartDate, context.Quarter).Amount;
        return TierSchedule.Lines(credited, proratedQuota).Sum(tier => tier.Amount);
    }
}
