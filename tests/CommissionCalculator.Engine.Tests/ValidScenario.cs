namespace CommissionCalculator.Engine.Tests;

// A scenario valid under every FR-004/FR-011/FR-013 rule, including rules later phases add
// (tasks standing rule 8): Appendix A.10's inputs. Tests change one thing at a time.
internal static class ValidScenario
{
    public static readonly DateOnly Past = new(2025, 6, 1);
    public static readonly QuarterPeriod Q1 = new(new(2026, 1, 1), new(2026, 3, 31));
    public static readonly QuarterPeriod Q2 = new(new(2026, 4, 1), new(2026, 6, 30));

    public static ScenarioInput Create() => new(
        "refunds-q2",
        "Refunds of earlier-quarter deals",
        Q2,
        [
            new RepInput("sage", "Sage", 100_000.00m, Past),
            new RepInput("val", "Val", 100_000.00m, Past),
            new RepInput("zion", "Zion", 100_000.00m, Past),
        ],
        [Deal("Q2-1", 40_000.00m, new(2026, 5, 1), new(2026, 5, 4), [new("sage", 100m)])],
        [
            new BookingQuarterInput(
                Q1,
                [new("sage", 100_000.00m, Past), new("val", 100_000.00m, Past), new("zion", 100_000.00m, Past)],
                [new("yves", Past)],
                [
                    Deal("R-11", 60_000.00m, new(2026, 1, 9), new(2026, 1, 10), [new("sage", 100m)], [new(60_000.00m, new(2026, 4, 15))]),
                    Deal("R-12", 60_000.00m, new(2026, 2, 9), new(2026, 2, 10), [new("sage", 100m)]),
                    Deal("R-7", 60_000.00m, new(2026, 1, 9), new(2026, 1, 10), [new("val", 100m)],
                        [new(20_000.00m, new(2026, 2, 20)), new(20_000.00m, new(2026, 5, 5))]),
                    Deal("R-8", 60_000.00m, new(2026, 2, 9), new(2026, 2, 10), [new("val", 100m)]),
                    Deal("D1", 60_000.00m, new(2026, 1, 14), new(2026, 1, 15), [new("zion", 100m)], [new(60_000.00m, new(2026, 4, 20))]),
                    Deal("D2", 40_000.00m, new(2026, 2, 24), new(2026, 2, 25), [new("zion", 50m), new("yves", 50m)]),
                ]),
        ]);

    public static DealInput Deal(string id, decimal amount, DateOnly close, DateOnly booking,
        IReadOnlyList<SplitCredit> splits, IReadOnlyList<RefundInput>? refunds = null) =>
        new(id, amount, close, booking, splits, refunds ?? []);

    public static BookingQuarterInput BookingQuarter(this ScenarioInput scenario) => scenario.BookingQuarters[0];

    public static ScenarioInput WithBookingQuarter(this ScenarioInput scenario, BookingQuarterInput bookingQuarter) =>
        scenario with { BookingQuarters = [bookingQuarter] };

    public static ScenarioInput WithBookingDeal(this ScenarioInput scenario, int index, Func<DealInput, DealInput> change)
    {
        var deals = scenario.BookingQuarter().Deals.ToList();
        deals[index] = change(deals[index]);
        return scenario.WithBookingQuarter(scenario.BookingQuarter() with { Deals = deals });
    }

    public static ScenarioInput WithOwnDeal(this ScenarioInput scenario, Func<DealInput, DealInput> change) =>
        scenario with { Deals = [change(scenario.Deals[0])] };
}
