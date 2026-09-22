namespace CommissionCalculator.Engine.Calculation;

[Implements("FR-003")]
internal static class StatementBuilder
{
    public static RepStatement Build(RepInput rep)
    {
        List<BreakdownLine> lines = [QuotaLine(rep)];
        return new RepStatement(rep.RepId, rep.Name,
            Quota: rep.Quota, ProratedQuota: rep.Quota, CreditedBookings: 0m, Attainment: 0m,
            CommissionBeforeRefunds: 0m, Clawbacks: 0m, EarnedCommission: 0m,
            DrawPaid: 0m, Recovered: 0m, Payable: 0m, ClosingRecoverableBalance: 0m,
            lines);
    }

    [Implements("FR-005")]
    private static BreakdownLine QuotaLine(RepInput rep) =>
        new(LineSection.Quota, "Quarterly quota", rep.Quota, "FR-005");
}
