using CommissionCalculator.Engine.Calculation;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

// Clawbacks recompute the booking quarter with and without each refund, in refund-date order
// across deals (clarifications 2026-09-21).
public sealed class ClawbackCalculatorTests
{
    private static readonly DateOnly Jan10 = new(2026, 1, 10);
    private static readonly DateOnly Feb10 = new(2026, 2, 10);

    private static ScenarioInput Quarter1(decimal quota, params DealInput[] deals) =>
        new("q1", "Q1", ValidScenario.Q1, [new RepInput("rep", "Rep", quota, ValidScenario.Past)], deals, []);

    private static DealInput Deal(string id, decimal amount, DateOnly booking, IReadOnlyList<RefundInput> refunds, params SplitCredit[] splits) =>
        new(id, amount, booking.AddDays(-1), booking, splits.Length == 0 ? [new SplitCredit("rep", 100m)] : splits, refunds);

    [Fact, Trait("Requirement", "FR-016")]
    public void AFullRefundClawsBackWhatTheQuarterWouldHaveBeenWithoutTheDeal()
    {
        var scenario = Quarter1(100_000.00m,
            Deal("A", 60_000.00m, Jan10, [new RefundInput(60_000.00m, new(2026, 2, 15))]),
            Deal("B", 60_000.00m, Feb10, []));

        var clawbacks = ClawbackCalculator.For(scenario.Roster[0], scenario);

        Assert.Equal([3_600.00m], clawbacks.Select(entry => entry.Amount));
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void APartialRefundClawsBackOnlyTheRefundedRevenue()
    {
        var scenario = Quarter1(100_000.00m,
            Deal("A", 60_000.00m, Jan10, [new RefundInput(30_000.00m, new(2026, 3, 1))]),
            Deal("B", 60_000.00m, Feb10, []));

        Assert.Equal([2_100.00m], ClawbackCalculator.For(scenario.Roster[0], scenario).Select(entry => entry.Amount));
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void RepeatedRefundsAreSizedAfterTheEarlierOnes()
    {
        var scenario = Quarter1(100_000.00m,
            Deal("A", 60_000.00m, Jan10, [new RefundInput(20_000.00m, new(2026, 2, 20)), new RefundInput(20_000.00m, new(2026, 3, 5))]),
            Deal("B", 60_000.00m, Feb10, []));

        Assert.Equal([1_600.00m, 1_000.00m], ClawbackCalculator.For(scenario.Roster[0], scenario).Select(entry => entry.Amount));
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void RefundsAcrossDealsAreOrderedByDateAndSizedInSequence()
    {
        var scenario = Quarter1(100_000.00m,
            Deal("A", 60_000.00m, Jan10, [new RefundInput(60_000.00m, new(2026, 2, 20))]),
            Deal("B", 60_000.00m, new(2026, 1, 13), [new RefundInput(60_000.00m, new(2026, 3, 10))]));

        var clawbacks = ClawbackCalculator.For(scenario.Roster[0], scenario);

        Assert.Equal([3_600.00m, 3_000.00m], clawbacks.Select(entry => entry.Amount));
        Assert.Equal(["A", "B"], clawbacks.Select(entry => entry.Deal.DealId));
    }

    [Fact, Trait("Requirement", "FR-016")]
    public void ARefundDatedAfterTheQuarterIsIgnored()
    {
        var scenario = Quarter1(100_000.00m,
            Deal("A", 60_000.00m, Jan10, [new RefundInput(20_000.00m, new(2026, 4, 20))]),
            Deal("B", 60_000.00m, Feb10, []));

        Assert.Empty(ClawbackCalculator.For(scenario.Roster[0], scenario));
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void ASplitRefundResplitsTheReducedDeal()
    {
        var scenario = new ScenarioInput("x", "X", ValidScenario.Q1,
            [new RepInput("xan", "Xan", 100_000.00m, ValidScenario.Past), new RepInput("yael", "Yael", 100_000.00m, ValidScenario.Past)],
            [Deal("X-1", 10_000.20m, new(2026, 1, 7), [new RefundInput(100.01m, Feb10)], new SplitCredit("xan", 50m), new SplitCredit("yael", 50m))],
            []);

        var xan = Assert.Single(ClawbackCalculator.For(scenario.Roster[0], scenario));
        var yael = Assert.Single(ClawbackCalculator.For(scenario.Roster[1], scenario));

        Assert.Equal((4_950.10m, 2.50m), (xan.ResplitShare, xan.Amount));
        Assert.Equal((4_950.09m, 2.51m), (yael.ResplitShare, yael.Amount));
        Assert.Equal(9_900.19m, xan.ReducedAmount);
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void AResplitThatRaisesAShareGivesANegativeClawback()
    {
        var scenario = new ScenarioInput("c", "C", ValidScenario.Q1,
            [new RepInput("ari", "Ari", 100_000.00m, ValidScenario.Past), new RepInput("bo", "Bo", 100_000.00m, ValidScenario.Past),
             new RepInput("cy", "Cy", 100_000.00m, ValidScenario.Past)],
            [Deal("X-2", 10.09m, new(2026, 1, 7), [], new SplitCredit("cy", 100m)),
             Deal("X-3", 0.06m, new(2026, 1, 9), [new RefundInput(0.01m, new(2026, 2, 11))],
                 new SplitCredit("ari", 45m), new SplitCredit("bo", 45m), new SplitCredit("cy", 10m))],
            []);

        var cy = Assert.Single(ClawbackCalculator.For(scenario.Roster[2], scenario));

        Assert.Equal((0.01m, -0.01m), (cy.ResplitShare, cy.Amount));
    }

    [Fact, Trait("Requirement", "FR-016")]
    public void AnEarlierQuartersRefundIsSizedFromThatQuartersData()
    {
        var q1 = new BookingQuarterInput(ValidScenario.Q1,
            [new BookingQuarterRep("rep", 100_000.00m, ValidScenario.Past)], [],
            [Deal("R-11", 60_000.00m, Jan10, [new RefundInput(60_000.00m, new(2026, 4, 15))]),
             Deal("R-12", 60_000.00m, Feb10, [])]);
        var scenario = new ScenarioInput("q2", "Q2", ValidScenario.Q2,
            [new RepInput("rep", "Rep", 100_000.00m, ValidScenario.Past)],
            [Deal("Q2-1", 40_000.00m, new(2026, 5, 4), [])],
            [q1]);

        Assert.Equal([3_600.00m], ClawbackCalculator.For(scenario.Roster[0], scenario).Select(entry => entry.Amount));
    }
}
