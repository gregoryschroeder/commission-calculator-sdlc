namespace CommissionCalculator.Engine;

public abstract record ScenarioResult;

public sealed record RejectedScenario(IReadOnlyList<ValidationError> Errors) : ScenarioResult;

public sealed record CalculatedScenario(IReadOnlyList<RepStatement> Statements) : ScenarioResult;

public sealed record ValidationError(string Message, string RequirementId);

public sealed record RepStatement(string RepId, string Name,
    decimal Quota, decimal ProratedQuota, decimal CreditedBookings, decimal Attainment,
    decimal CommissionBeforeRefunds, decimal Clawbacks, decimal EarnedCommission,
    decimal DrawPaid, decimal Recovered, decimal Payable, decimal ClosingRecoverableBalance,
    IReadOnlyList<BreakdownLine> Lines);

public enum LineSection { Quota, Credit, Excluded, Subtotal, Tier, Resplit, Clawback, Draw, Recovery }

public sealed record BreakdownLine(LineSection Section, string Description, decimal Amount,
    string RequirementId);
