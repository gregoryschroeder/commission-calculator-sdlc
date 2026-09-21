# Contract: Commission engine public API

The engine (`CommissionCalculator.Engine`) is a library with no I/O. This is the whole public
surface the web app and the tests use; everything else is `internal`. Names are the planned C#
names; shapes, not bodies.

```csharp
namespace CommissionCalculator.Engine;

public sealed record QuarterPeriod(DateOnly Start, DateOnly End);

public sealed record RepInput(string RepId, string Name, decimal Quota, DateOnly StartDate,
    decimal OpeningRecoverableBalance = 0m);

public sealed record SplitCredit(string RepId, decimal Percent);
public sealed record RefundInput(decimal Amount, DateOnly Date);

public sealed record DealInput(string DealId, decimal Amount, DateOnly CloseDate,
    DateOnly BookingDate, IReadOnlyList<SplitCredit> Splits, IReadOnlyList<RefundInput> Refunds);

public sealed record BookingQuarterRep(string RepId, decimal Quota, DateOnly StartDate);
public sealed record Partner(string RepId, DateOnly StartDate);

public sealed record BookingQuarterInput(QuarterPeriod Quarter,
    IReadOnlyList<BookingQuarterRep> Reps, IReadOnlyList<Partner> Partners,
    IReadOnlyList<DealInput> Deals);

public sealed record ScenarioInput(string Id, string Name, QuarterPeriod Quarter,
    IReadOnlyList<RepInput> Roster, IReadOnlyList<DealInput> Deals,
    IReadOnlyList<BookingQuarterInput> BookingQuarters);

public static class CommissionEngine
{
    // Validates, then calculates. Never throws for invalid scenario data: invalid data is a
    // Rejected result (FR-004). Deterministic: same input, same output.
    public static ScenarioResult Calculate(ScenarioInput scenario);
}

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

// Marks the member that implements a requirement; read by the tools project's `trace` command.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ImplementsAttribute(string requirementId) : Attribute
{
    public string RequirementId { get; } = requirementId;
}
```

## Guarantees

- Every monetary field and line amount is a whole number of cents (FR-018).
- `Lines` contains every amount in the statement; each `RequirementId` is an FR in spec.md
  (FR-003, SC-002). Subtotals equal the sum of the lines they total (FR-018).
- `Attainment` is `CreditedBookings / ProratedQuota` and is never used to compute an amount.
- A `RejectedScenario` lists every validation failure found, not only the first.
