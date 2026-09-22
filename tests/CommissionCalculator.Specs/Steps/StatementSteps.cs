using System.Globalization;
using CommissionCalculator.Engine;
using Reqnroll;
using Xunit;

namespace CommissionCalculator.Specs.Steps;

// Builds a scenario from Gherkin tables and checks the engine's statements. Lines are matched as
// an ordered subsequence: later phases add lines to every statement, and a test is never edited
// to pass (tasks T027).
[Binding]
public sealed class StatementSteps
{
    private QuarterPeriod? _quarter;
    private List<RepInput> _roster = [];
    private List<DealInput> _deals = [];
    private CalculatedScenario? _result;
    private RejectedScenario? _rejected;

    [Given("the quarter {word} to {word}")]
    public void GivenTheQuarter(string start, string end) => _quarter = new(Date(start), Date(end));

    [Given("the roster:")]
    public void GivenTheRoster(DataTable table) =>
        _roster = table.Rows.Select(row => new RepInput(row["rep"], row["name"], Amount(row["quota"]), Date(row["start"]),
            row.TryGetValue("opening", out var opening) ? Amount(opening) : 0m)).ToList();

    [Given("the deals:")]
    public void GivenTheDeals(DataTable table) =>
        _deals = table.Rows.Select(row => new DealInput(row["deal"], Amount(row["amount"]), Date(row["close"]), Date(row["booking"]),
            Credits(row["credit"]), [])).ToList();

    [When("the scenario is calculated")]
    public void WhenTheScenarioIsCalculated()
    {
        var quarter = _quarter ?? throw new InvalidOperationException("No quarter was given.");
        var result = CommissionEngine.Calculate(new ScenarioInput("feature", "Feature scenario", quarter, _roster, _deals, []));
        _result = Assert.IsType<CalculatedScenario>(result);
    }

    [When("the scenario is calculated it is rejected")]
    public void WhenTheScenarioIsRejected()
    {
        var quarter = _quarter ?? throw new InvalidOperationException("No quarter was given.");
        var result = CommissionEngine.Calculate(new ScenarioInput("feature", "Feature scenario", quarter, _roster, _deals, []));
        _rejected = Assert.IsType<RejectedScenario>(result);
    }

    [Then("the rejection names deal {string} and cites {string}")]
    public void ThenTheRejectionNamesDeal(string dealId, string requirement) =>
        Assert.Contains(Rejected.Errors, error =>
            error.RequirementId == requirement && error.Message.Contains(dealId, StringComparison.Ordinal));

    [Then("the rejection says {string}")]
    public void ThenTheRejectionSays(string phrase) =>
        Assert.Contains(Rejected.Errors, error => error.Message.Contains(phrase, StringComparison.Ordinal));

    [Then("the statement for {string} includes, in order:")]
    public void ThenTheStatementIncludesInOrder(string repId, DataTable table)
    {
        var expected = table.Rows.Select(row => (row["item"], Amount(row["amount"]), row["rule"])).ToList();
        var actual = Statement(repId).Lines.Select(line => (line.Description, line.Amount, line.RequirementId)).ToList();

        var next = 0;
        foreach (var line in actual)
        {
            if (next < expected.Count && line == expected[next])
            {
                next++;
            }
        }

        Assert.True(next == expected.Count,
            $"Missing or out of order: {expected[Math.Min(next, expected.Count - 1)]}\nActual lines:\n{string.Join("\n", actual)}");
    }

    [Then("the statement for {string} has no line starting {string}")]
    public void ThenTheStatementHasNoLineStarting(string repId, string prefix) =>
        Assert.DoesNotContain(Statement(repId).Lines, line => line.Description.StartsWith(prefix, StringComparison.Ordinal));

    [Then("the statement for {string} has:")]
    public void ThenTheStatementHas(string repId, DataTable table)
    {
        var statement = Statement(repId);
        foreach (var row in table.Rows)
        {
            var property = typeof(RepStatement).GetProperty(row["field"])
                ?? throw new InvalidOperationException($"RepStatement has no field '{row["field"]}'.");
            Assert.Equal(Amount(row["value"]), (decimal)property.GetValue(statement)!);
        }
    }

    private RejectedScenario Rejected => _rejected ?? throw new InvalidOperationException("The scenario was not rejected.");

    private RepStatement Statement(string repId) =>
        (_result ?? throw new InvalidOperationException("The scenario was not calculated."))
        .Statements.Single(statement => statement.RepId == repId);

    private static List<SplitCredit> Credits(string text) =>
        text.Split(',', StringSplitOptions.TrimEntries)
            .Select(part => part.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(parts => new SplitCredit(parts[0], Amount(parts[1].TrimEnd('%'))))
            .ToList();

    private static DateOnly Date(string text) => DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static decimal Amount(string text) =>
        decimal.Parse(text.Replace("−", "-", StringComparison.Ordinal).Replace("$", "", StringComparison.Ordinal),
            NumberStyles.Number, CultureInfo.InvariantCulture);
}
