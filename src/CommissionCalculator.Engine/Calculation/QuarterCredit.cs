namespace CommissionCalculator.Engine.Calculation;

internal sealed record DealCredit(DealInput Deal, decimal Share, bool Counted);

// A deal counts toward the quarter that contains its booking date (clarification 2026-09-21,
// FR-008); the close date never affects crediting. Deals outside the quarter are listed with a
// zero share so the statement can say why they were excluded.
[Implements("FR-008")]
internal static class QuarterCredit
{
    public static IReadOnlyList<DealCredit> For(string repId, QuarterPeriod quarter, IReadOnlyList<DealInput> deals) =>
        deals.Where(deal => deal.Splits.Any(split => split.RepId == repId))
            .Select(deal => IsBookedIn(deal, quarter)
                ? new DealCredit(deal, deal.Amount, Counted: true)
                : new DealCredit(deal, 0m, Counted: false))
            .ToList();

    private static bool IsBookedIn(DealInput deal, QuarterPeriod quarter) =>
        deal.BookingDate >= quarter.Start && deal.BookingDate <= quarter.End;
}
