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
