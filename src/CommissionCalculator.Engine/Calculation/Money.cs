namespace CommissionCalculator.Engine.Calculation;

internal static class Money
{
    [Implements("FR-018")]
    public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    [Implements("FR-004")]
    public static bool IsWholeCents(decimal amount) => decimal.Round(amount, 2) == amount;
}
