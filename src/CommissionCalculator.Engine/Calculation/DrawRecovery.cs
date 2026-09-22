namespace CommissionCalculator.Engine.Calculation;

internal sealed record Recovery(decimal Recovered, decimal Payable, decimal ClosingBalance);

// Draw is recoverable: the opening balance plus this quarter's draws are recovered from earned
// commission, what is left over is carried forward, and commission payable is never negative
// (clarification 2026-09-21, FR-015). Negative earnings add to the balance instead.
[Implements("FR-015")]
internal static class DrawRecovery
{
    public static Recovery For(decimal earnedCommission, decimal openingBalance, decimal drawPaid)
    {
        var recoverableTotal = openingBalance + drawPaid;
        if (earnedCommission < 0m)
        {
            return new Recovery(0.00m, 0.00m, recoverableTotal - earnedCommission);
        }

        var recovered = Math.Min(earnedCommission, recoverableTotal);
        return new Recovery(recovered, earnedCommission - recovered, recoverableTotal - recovered);
    }
}
