using CommissionCalculator.Engine.Validation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

// FR-004's shared rules, each tested in every place it applies (tasks standing rule 8). Each test
// asserts that its own error is among those listed, not an exact error set, because later phases
// add rules that some of these inputs also break.
public sealed class ScenarioValidatorTests
{
    // Requirement defaults to FR-004; rules that belong to another requirement say so, as
    // Appendix A.11 states them (corrected at the Phase 6 gate, 2026-09-22).
    public sealed record RuleCase(string Name, Func<ScenarioInput, ScenarioInput> Break, string Phrase, string Subject, string Requirement = "FR-004")
    {
        public override string ToString() => Name;
    }

    private static readonly DateOnly May10 = new(2026, 5, 10);

    private static readonly RuleCase[] Cases =
    [
        new("roster quota zero", s => s with { Roster = [s.Roster[0] with { Quota = 0m }, .. s.Roster.Skip(1)] }, "quota must be greater than zero", "sage"),
        new("roster quota negative", s => s with { Roster = [s.Roster[0] with { Quota = -1m }, .. s.Roster.Skip(1)] }, "quota must be greater than zero", "sage"),
        new("booking-quarter quota zero", s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [s.BookingQuarter().Reps[0] with { Quota = 0m }, .. s.BookingQuarter().Reps.Skip(1)] }), "quota must be greater than zero", "sage"),
        new("own deal amount zero", s => s.WithOwnDeal(d => d with { Amount = 0m }), "amount must be greater than zero", "Q2-1"),
        new("booking-quarter deal amount negative", s => s.WithBookingDeal(1, d => d with { Amount = -5m }), "amount must be greater than zero", "R-12"),
        new("own refund amount zero", s => s.WithOwnDeal(d => d with { Refunds = [new(0m, May10)] }), "refund amount must be greater than zero", "Q2-1"),
        new("booking-quarter refund amount zero", s => s.WithBookingDeal(1, d => d with { Refunds = [new(0m, new(2026, 2, 20))] }), "refund amount must be greater than zero", "R-12"),
        new("roster quota sub-cent", s => s with { Roster = [s.Roster[0] with { Quota = 100_000.005m }, .. s.Roster.Skip(1)] }, "is not a whole number of cents", "sage"),
        new("booking-quarter quota sub-cent", s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [s.BookingQuarter().Reps[0] with { Quota = 100_000.005m }, .. s.BookingQuarter().Reps.Skip(1)] }), "is not a whole number of cents", "sage"),
        new("own deal amount sub-cent", s => s.WithOwnDeal(d => d with { Amount = 10.005m }), "is not a whole number of cents", "Q2-1"),
        new("booking-quarter deal amount sub-cent", s => s.WithBookingDeal(1, d => d with { Amount = 10.005m }), "is not a whole number of cents", "R-12"),
        new("own refund amount sub-cent", s => s.WithOwnDeal(d => d with { Refunds = [new(1.005m, May10)] }), "is not a whole number of cents", "Q2-1"),
        new("booking-quarter refund amount sub-cent", s => s.WithBookingDeal(1, d => d with { Refunds = [new(1.005m, new(2026, 2, 20))] }), "is not a whole number of cents", "R-12"),
        new("opening balance sub-cent", s => s with { Roster = [s.Roster[0] with { OpeningRecoverableBalance = 0.001m }, .. s.Roster.Skip(1)] }, "is not a whole number of cents", "sage"),
        new("opening balance negative", s => s with { Roster = [s.Roster[0] with { OpeningRecoverableBalance = -1_000.00m }, .. s.Roster.Skip(1)] }, "opening recoverable balance must not be negative", "sage"),
        new("empty roster", s => s with { Roster = [], Deals = [], BookingQuarters = [] }, "roster is empty", "refunds-q2"),
        new("scenario quarter not three whole months", s => s with { Quarter = new(new(2026, 4, 2), new(2026, 6, 30)) }, "is not three whole calendar months", "2026-04-02"),
        new("booking quarter not three whole months", s => s.WithBookingQuarter(s.BookingQuarter() with { Quarter = new(new(2026, 1, 1), new(2026, 2, 28)) }), "is not three whole calendar months", "2026-01-01"),
        new("own deal credited to a rep not on the roster", s => s.WithOwnDeal(d => d with { Splits = [new("nobody", 100m)] }), "is not on the roster", "nobody"),
        new("same rep twice on an own deal", s => s.WithOwnDeal(d => d with { Splits = [new("sage", 50m), new("sage", 50m)] }), "appears more than once on deal", "Q2-1"),
        new("same rep twice on a booking-quarter deal", s => s.WithBookingDeal(5, d => d with { Splits = [new("zion", 50m), new("zion", 50m)] }), "appears more than once on deal", "D2"),
        new("duplicate repId on the roster", s => s with { Roster = [.. s.Roster, s.Roster[0] with { Name = "Sage again" }] }, "appears more than once on the roster", "sage"),
        new("duplicate dealId in the own deal list", s => s with { Deals = [s.Deals[0], s.Deals[0]] }, "appears more than once in the deal list", "Q2-1"),
        new("duplicate dealId in a booking-quarter deal list", s => s.WithBookingDeal(1, d => d with { DealId = "R-11" }), "appears more than once in the deal list", "R-11"),
        new("own split percentage zero", s => s.WithOwnDeal(d => d with { Splits = [new("sage", 0m), new("val", 100m)] }), "split percentage must be greater than 0% and at most 100%", "Q2-1"),
        new("own split percentage over 100", s => s.WithOwnDeal(d => d with { Splits = [new("sage", 100.5m)] }), "split percentage must be greater than 0% and at most 100%", "Q2-1"),
        new("booking-quarter split percentage zero", s => s.WithBookingDeal(5, d => d with { Splits = [new("zion", 0m), new("yves", 100m)] }), "split percentage must be greater than 0% and at most 100%", "D2"),
        new("booking-quarter split percentage over 100", s => s.WithBookingDeal(1, d => d with { Splits = [new("sage", 101m)] }), "split percentage must be greater than 0% and at most 100%", "R-12"),
        new("own split percentages sum below 100", s => s.WithOwnDeal(d => d with { Splits = [new("sage", 99.999m)] }), "split percentages sum to 99.999%", "Q2-1", "FR-013"),
        new("own split percentages sum above 100", s => s.WithOwnDeal(d => d with { Splits = [new("sage", 100.001m)] }), "split percentages sum to 100.001%", "Q2-1", "FR-013"),
        new("booking-quarter split percentages sum below 100", s => s.WithBookingDeal(1, d => d with { Splits = [new("sage", 99.999m)] }), "split percentages sum to 99.999%", "R-12", "FR-013"),
        new("booking-quarter split percentages sum above 100", s => s.WithBookingDeal(5, d => d with { Splits = [new("zion", 60m), new("yves", 40.001m)] }), "split percentages sum to 100.001%", "D2", "FR-013"),
        new("earlier-quarter deal refunded this quarter with no booking-quarter data",
            s => s with { Deals = [.. s.Deals, ValidScenario.Deal("OLD", 10_000.00m, new(2025, 10, 4), new(2025, 10, 5), [new("sage", 100m)], [new(1_000.00m, new(2026, 5, 6))])] },
            "has no booking-quarter data", "OLD"),
        new("roster rep credited on a refunded earlier-quarter deal with no rep entry",
            s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [.. s.BookingQuarter().Reps.Where(rep => rep.RepId != "sage")] }),
            "has no entry in the booking quarter", "sage"),
        new("split partner on a booking-quarter deal with no start date",
            s => s.WithBookingQuarter(s.BookingQuarter() with { Partners = [] }), "is neither on the roster nor listed as a partner", "yves"),
        new("booking quarter overlaps the scenario quarter",
            s => s.WithBookingQuarter(s.BookingQuarter() with { Quarter = ValidScenario.Q2 }), "overlaps the scenario's quarter", "2026-04-01"),
        new("booking-quarter deal booked outside that quarter",
            s => s.WithBookingDeal(1, d => d with { BookingDate = new(2026, 5, 10) }), "is booked outside its booking quarter", "R-12"),
        new("booking-quarter start date differs from the roster",
            s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [s.BookingQuarter().Reps[0] with { StartDate = new(2025, 7, 1) }, .. s.BookingQuarter().Reps.Skip(1)] }),
            "start date differs from the roster", "sage"),
        new("a deal listed in both lists differs between them",
            s => s with { Deals = [.. s.Deals, s.BookingQuarter().Deals[0] with { Amount = 59_000.00m }] }, "differs between the two lists", "R-11"),
        new("own refund dated before booking", s => s.WithOwnDeal(d => d with { Refunds = [new(1_000.00m, new(2026, 5, 1))] }), "is dated before its booking date", "Q2-1"),
        new("booking-quarter refund dated before booking", s => s.WithBookingDeal(1, d => d with { Refunds = [new(1_000.00m, new(2026, 2, 1))] }), "is dated before its booking date", "R-12"),
        new("own refunds total more than the deal", s => s.WithOwnDeal(d => d with { Refunds = [new(30_000.00m, May10), new(20_000.00m, May10)] }), "refunds total more than the deal amount", "Q2-1"),
        new("prorated quota rounds to zero", s => s with { Roster = [s.Roster[0] with { Quota = 0.01m, StartDate = new(2026, 6, 30) }, .. s.Roster.Skip(1)] }, "prorated quota rounds to $0.00", "sage"),
        new("booking-quarter prorated quota rounds to zero", s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [s.BookingQuarter().Reps[0] with { Quota = 0.01m, StartDate = new(2026, 3, 31) }, .. s.BookingQuarter().Reps.Skip(1)] }), "prorated quota rounds to $0.00", "sage"),
        new("rep starts after the quarter ends", s => s with { Roster = [s.Roster[0] with { StartDate = new(2026, 7, 1) }, .. s.Roster.Skip(1)] }, "starts after the quarter ends", "sage", "FR-011"),
        new("booking-quarter rep starts after that quarter ends", s => s.WithBookingQuarter(s.BookingQuarter() with { Reps = [s.BookingQuarter().Reps[0] with { StartDate = new(2026, 4, 1) }, .. s.BookingQuarter().Reps.Skip(1)] }), "starts after the quarter ends", "sage", "FR-011"),
        new("own deal booked before its rep's start date", s => s with { Roster = [s.Roster[0] with { StartDate = new(2026, 5, 10) }, .. s.Roster.Skip(1)] }, "is booked before the start date of rep", "Q2-1", "FR-011"),
        new("booking-quarter deal booked before a partner's start date", s => s.WithBookingQuarter(s.BookingQuarter() with { Partners = [new("yves", new(2026, 3, 1))] }), "is booked before the start date of rep", "D2", "FR-011"),
        new("booking-quarter refunds total more than the deal", s => s.WithBookingDeal(5, d => d with { Refunds = [new(25_000.00m, new(2026, 3, 1)), new(20_000.00m, new(2026, 3, 2))] }), "refunds total more than the deal amount", "D2"),
    ];

    public static TheoryData<RuleCase> Rules => [.. Cases];

    [Fact, Trait("Requirement", "FR-004")]
    public void TheValidScenarioHasNoErrors() => Assert.Empty(ScenarioValidator.Validate(ValidScenario.Create()));

    [Theory, Trait("Requirement", "FR-004"), MemberData(nameof(Rules))]
    public void EachSharedRuleRejectsItsViolation(RuleCase rule)
    {
        var errors = ScenarioValidator.Validate(rule.Break(ValidScenario.Create()));

        Assert.Contains(errors, error =>
            error.RequirementId == rule.Requirement
            && error.Message.Contains(rule.Phrase, StringComparison.Ordinal)
            && error.Message.Contains(rule.Subject, StringComparison.Ordinal));
    }

    [Fact, Trait("Requirement", "FR-004")]
    public void APartnerListedWithAStartDateIsAccepted() =>
        Assert.DoesNotContain(ScenarioValidator.Validate(ValidScenario.Create()),
            error => error.Message.Contains("yves", StringComparison.Ordinal));

    [Fact, Trait("Requirement", "FR-013")]
    public void SplitPercentagesSummingToExactlyOneHundredAreAccepted() =>
        Assert.DoesNotContain(ScenarioValidator.Validate(ValidScenario.Create()),
            error => error.Message.Contains("split percentages sum", StringComparison.Ordinal));

    [Fact, Trait("Requirement", "FR-004")]
    public void EveryViolationIsListedNotOnlyTheFirst()
    {
        var scenario = ValidScenario.Create();
        scenario = scenario with
        {
            Roster = [scenario.Roster[0] with { Quota = 0m }, scenario.Roster[1] with { OpeningRecoverableBalance = -1m }, scenario.Roster[2]],
        };
        scenario = scenario.WithOwnDeal(d => d with { Amount = 40_000.005m });

        var messages = ScenarioValidator.Validate(scenario).Select(error => error.Message).ToList();

        Assert.Contains(messages, m => m.Contains("quota must be greater than zero", StringComparison.Ordinal));
        Assert.Contains(messages, m => m.Contains("opening recoverable balance must not be negative", StringComparison.Ordinal));
        Assert.Contains(messages, m => m.Contains("is not a whole number of cents", StringComparison.Ordinal));
    }
}
