namespace CommissionCalculator.Engine.Calculation;

internal sealed record CreditLine(DealInput Deal, decimal Share);

[Implements("FR-008")]
internal static class QuarterCredit
{
    public static IReadOnlyList<CreditLine> For(string repId, IReadOnlyList<DealInput> deals) =>
        deals.Where(deal => deal.Splits.Any(split => split.RepId == repId))
            .Select(deal => new CreditLine(deal, deal.Amount))
            .ToList();
}
