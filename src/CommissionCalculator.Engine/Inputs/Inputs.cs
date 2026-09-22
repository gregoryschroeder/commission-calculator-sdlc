namespace CommissionCalculator.Engine;

public sealed record QuarterPeriod(DateOnly Start, DateOnly End);

public sealed record RepInput(string RepId, string Name, decimal Quota, DateOnly StartDate,
    decimal OpeningRecoverableBalance = 0m);

public sealed record SplitCredit(string RepId, decimal Percent);

public sealed record RefundInput(decimal Amount, DateOnly Date);

public sealed record DealInput(string DealId, decimal Amount, DateOnly CloseDate,
    DateOnly BookingDate, IReadOnlyList<SplitCredit> Splits, IReadOnlyList<RefundInput> Refunds)
{
    // The generated == compares Splits and Refunds by reference, so two deals read from the same
    // JSON never come out equal. Compare the values instead wherever "the same deal" is the
    // question (FR-004's rule about a deal listed in both the scenario's list and a booking
    // quarter's).
    public static bool HaveEqualValues(DealInput first, DealInput second) =>
        first.DealId == second.DealId
        && first.Amount == second.Amount
        && first.CloseDate == second.CloseDate
        && first.BookingDate == second.BookingDate
        && first.Splits.SequenceEqual(second.Splits)
        && first.Refunds.SequenceEqual(second.Refunds);
}

public sealed record BookingQuarterRep(string RepId, decimal Quota, DateOnly StartDate);

public sealed record Partner(string RepId, DateOnly StartDate);

public sealed record BookingQuarterInput(QuarterPeriod Quarter,
    IReadOnlyList<BookingQuarterRep> Reps, IReadOnlyList<Partner> Partners,
    IReadOnlyList<DealInput> Deals);

public sealed record ScenarioInput(string Id, string Name, QuarterPeriod Quarter,
    IReadOnlyList<RepInput> Roster, IReadOnlyList<DealInput> Deals,
    IReadOnlyList<BookingQuarterInput> BookingQuarters);
