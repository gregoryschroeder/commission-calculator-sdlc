namespace CommissionCalculator.Engine.Calculation;

// Largest remainder (clarification 2026-09-21, FR-012): each share is rounded down to the cent,
// then the leftover cents go one at a time to the largest fractional remainders, ties to the rep
// listed first. Shares therefore always sum to the amount split.
[Implements("FR-012")]
internal static class SplitAllocation
{
    private const decimal Cent = 0.01m;

    public static IReadOnlyList<decimal> Allocate(decimal amount, IReadOnlyList<SplitCredit> splits)
    {
        var exact = splits.Select(split => amount * split.Percent / 100m).ToList();
        var shares = exact.Select(share => decimal.Floor(share / Cent) * Cent).ToList();
        var leftover = (int)decimal.Round((amount - shares.Sum()) / Cent);

        var order = Enumerable.Range(0, splits.Count)
            .OrderByDescending(index => exact[index] - shares[index])
            .ThenBy(index => index);
        foreach (var index in order.Take(Math.Max(leftover, 0)))
        {
            shares[index] += Cent;
        }

        return shares;
    }
}
