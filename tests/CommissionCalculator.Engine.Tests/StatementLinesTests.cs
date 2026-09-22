using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed class StatementLinesTests
{
    private static IReadOnlyList<BreakdownLine> LinesFor(string repId, ScenarioInput scenario) =>
        Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(scenario))
            .Statements.Single(statement => statement.RepId == repId).Lines;

    [Fact, Trait("Requirement", "FR-017")]
    public void ARefundOnASplitDealAddsAReSplitLineBeforeItsClawback()
    {
        var scenario = new ScenarioInput("x", "X", ValidScenario.Q1,
            [new RepInput("xan", "Xan", 100_000.00m, ValidScenario.Past), new RepInput("yael", "Yael", 100_000.00m, ValidScenario.Past)],
            [ValidScenario.Deal("X-1", 10_000.20m, new(2026, 1, 6), new(2026, 1, 7),
                [new SplitCredit("xan", 50m), new SplitCredit("yael", 50m)], [new RefundInput(100.01m, new(2026, 2, 10))])],
            []);

        var descriptions = LinesFor("xan", scenario).Select(line => line.Description).ToList();
        var resplit = descriptions.FindIndex(description => description.StartsWith("X-1 re-split after refund", StringComparison.Ordinal));
        var clawback = descriptions.FindIndex(description => description.StartsWith("Clawback: X-1", StringComparison.Ordinal));

        Assert.True(resplit >= 0, "no re-split line");
        Assert.True(clawback > resplit, "the clawback line does not follow the re-split line");
    }

    [Fact, Trait("Requirement", "FR-017")]
    public void ARefundOnASingleRepDealAddsNoReSplitLine()
    {
        var scenario = new ScenarioInput("s", "S", ValidScenario.Q1,
            [new RepInput("sage", "Sage", 100_000.00m, ValidScenario.Past)],
            [ValidScenario.Deal("R-1", 60_000.00m, new(2026, 1, 9), new(2026, 1, 10), [new SplitCredit("sage", 100m)],
                [new RefundInput(60_000.00m, new(2026, 2, 15))])],
            []);

        Assert.DoesNotContain(LinesFor("sage", scenario), line => line.Description.Contains("re-split", StringComparison.Ordinal));
    }
}
