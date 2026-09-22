namespace CommissionCalculator.Engine.Calculation;

internal sealed record TierLine(decimal Rate, decimal Slice, decimal Amount);

// Marginal tiers (clarification 2026-09-21, FR-006): each slice of credit earns its own band's
// rate. The 150% edge is computed from the (already rounded) prorated quota and rounded itself
// (FR-018), so later lines use the rounded value.
[Implements("FR-006")]
[Implements("FR-007")]
internal static class TierSchedule
{
    private const decimal BaseRate = 0.05m;
    private const decimal MiddleRate = 0.08m;
    private const decimal TopRate = 0.12m;
    private const decimal TopBandStart = 1.5m;

    public static IReadOnlyList<TierLine> Lines(decimal credit, decimal proratedQuota)
    {
        var quotaEdge = proratedQuota;
        var topEdge = Money.Round(TopBandStart * proratedQuota);
        (decimal Rate, decimal Slice)[] slices =
        [
            (BaseRate, Math.Min(credit, quotaEdge)),
            (MiddleRate, Math.Clamp(credit - quotaEdge, 0m, topEdge - quotaEdge)),
            (TopRate, Math.Max(credit - topEdge, 0m)),
        ];

        return slices
            .Where(slice => slice.Slice > 0m)
            .Select(slice => new TierLine(slice.Rate, slice.Slice, Money.Round(slice.Rate * slice.Slice)))
            .ToList();
    }
}
