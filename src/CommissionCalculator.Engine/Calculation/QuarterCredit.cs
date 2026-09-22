namespace CommissionCalculator.Engine.Calculation;

internal readonly record struct RefundKey(string DealId, int Index)
{
    public static readonly IReadOnlySet<RefundKey> None = new HashSet<RefundKey>();
}

internal sealed record DealCredit(DealInput Deal, decimal Share, bool Counted, SplitCredit? Split);

// A deal counts toward the quarter that contains its booking date (clarification 2026-09-21,
// FR-008); the close date never affects crediting. Deals outside the quarter are listed with a
// zero share so the statement can say why they were excluded.
[Implements("FR-008")]
internal static class QuarterCredit
{
    public static IReadOnlyList<DealCredit> For(string repId, QuarterPeriod quarter, IReadOnlyList<DealInput> deals) =>
        For(repId, quarter, deals, RefundKey.None);

    public static IReadOnlyList<DealCredit> For(string repId, QuarterPeriod quarter, IReadOnlyList<DealInput> deals,
        IReadOnlySet<RefundKey> appliedRefunds) =>
        deals.Select(deal => (Deal: deal, Index: IndexOfRep(deal, repId)))
            .Where(entry => entry.Index >= 0)
            .Select(entry => Credit(entry.Deal, entry.Index, quarter, appliedRefunds))
            .ToList();

    // The amount a deal still carries once the given refunds have been applied (FR-017).
    public static decimal ReducedAmount(DealInput deal, IReadOnlySet<RefundKey> appliedRefunds) =>
        deal.Amount - deal.Refunds
            .Where((_, index) => appliedRefunds.Contains(new RefundKey(deal.DealId, index)))
            .Sum(refund => refund.Amount);

    private static DealCredit Credit(DealInput deal, int splitIndex, QuarterPeriod quarter, IReadOnlySet<RefundKey> appliedRefunds)
    {
        var split = deal.Splits.Count > 1 ? deal.Splits[splitIndex] : null;
        if (!IsBookedIn(deal, quarter))
        {
            return new DealCredit(deal, 0m, Counted: false, split);
        }

        var share = SplitAllocation.Allocate(ReducedAmount(deal, appliedRefunds), deal.Splits)[splitIndex];
        return new DealCredit(deal, share, Counted: true, split);
    }

    private static int IndexOfRep(DealInput deal, string repId)
    {
        for (var index = 0; index < deal.Splits.Count; index++)
        {
            if (deal.Splits[index].RepId == repId)
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsBookedIn(DealInput deal, QuarterPeriod quarter) =>
        deal.BookingDate >= quarter.Start && deal.BookingDate <= quarter.End;
}
