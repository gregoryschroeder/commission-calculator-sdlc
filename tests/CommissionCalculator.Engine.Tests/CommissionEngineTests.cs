using System.Text.RegularExpressions;
using Xunit;

namespace CommissionCalculator.Engine.Tests;

public sealed partial class CommissionEngineTests
{
    [Fact, Trait("Requirement", "FR-004")]
    public void AnInvalidScenarioIsRejectedWithNoStatements()
    {
        var invalid = ValidScenario.Create() with { Roster = [], Deals = [], BookingQuarters = [] };

        var result = CommissionEngine.Calculate(invalid);

        var rejected = Assert.IsType<RejectedScenario>(result);
        Assert.NotEmpty(rejected.Errors);
    }

    [Fact, Trait("Requirement", "FR-003")]
    public void AValidScenarioHasOneStatementPerRosterRepInRosterOrder()
    {
        var result = Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(ValidScenario.Create()));

        Assert.Equal(["sage", "val", "zion"], result.Statements.Select(statement => statement.RepId));
    }

    [Fact, Trait("Requirement", "FR-005")]
    public void EachStatementOpensWithTheQuarterlyQuota()
    {
        var result = Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(ValidScenario.Create()));

        Assert.All(result.Statements, statement =>
            Assert.Equal(new BreakdownLine(LineSection.Quota, "Quarterly quota", 100_000.00m, "FR-005"), statement.Lines[0]));
    }

    [Fact, Trait("Requirement", "FR-003")]
    public void EveryLineCitesARequirementId()
    {
        var result = Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(ValidScenario.Create()));

        Assert.All(result.Statements.SelectMany(statement => statement.Lines),
            line => Assert.Matches(RequirementId(), line.RequirementId));
    }

    [GeneratedRegex(@"^FR-\d{3}$")]
    private static partial Regex RequirementId();
}
