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
    private QuarterPeriod? _bookingQuarter;
    private List<BookingQuarterRep> _bookingReps = [];
    private List<Partner> _partners = [];
    private List<DealInput> _bookingDeals = [];
    private RejectedScenario? _rejected;

    [Given("the quarter {word} to {word}")]
    public void GivenTheQuarter(string start, string end) => _quarter = new(Date(start), Date(end));

    [Given("the roster:")]
    public void GivenTheRoster(DataTable table) =>
        _roster = table.Rows.Select(row => new RepInput(row["rep"], row["name"], Amount(row["quota"]), Date(row["start"]),
            row.TryGetValue("opening", out var opening) ? Amount(opening) : 0m)).ToList();

    [Given("the deals:")]
    public void GivenTheDeals(DataTable table) => _deals = Deals(table);

    [Given("the booking quarter {word} to {word} with reps:")]
    public void GivenTheBookingQuarterReps(string start, string end, DataTable table)
    {
        _bookingQuarter = new QuarterPeriod(Date(start), Date(end));
        _bookingReps = table.Rows.Select(row => new BookingQuarterRep(row["rep"], Amount(row["quota"]), Date(row["start"]))).ToList();
    }

    [Given("its partners:")]
    public void GivenItsPartners(DataTable table) =>
        _partners = table.Rows.Select(row => new Partner(row["rep"], Date(row["start"]))).ToList();

    [Given("its deals:")]
    public void GivenItsDeals(DataTable table) => _bookingDeals = Deals(table);

    private static List<DealInput> Deals(DataTable table) =>
        table.Rows.Select(row => new DealInput(row["deal"], Amount(row["amount"]), Date(row["close"]), Date(row["booking"]),
            Credits(row["credit"]),
            row.TryGetValue("refunds", out var refunds) ? Refunds(refunds) : [])).ToList();

    // "60,000.00 on 2026-02-15; 20,000.00 on 2026-05-05"
    private static List<RefundInput> Refunds(string text) =>
        text is "" or "-"
            ? []
            : text.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split(" on ", StringSplitOptions.TrimEntries))
                .Select(parts => new RefundInput(Amount(parts[0]), Date(parts[1])))
                .ToList();

    [When("the scenario is calculated")]
    public void WhenTheScenarioIsCalculated()
    {
        var quarter = _quarter ?? throw new InvalidOperationException("No quarter was given.");
        _result = Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(Scenario(quarter)));
    }

    [When("the scenario is calculated it is rejected")]
    public void WhenTheScenarioIsRejected()
    {
        var quarter = _quarter ?? throw new InvalidOperationException("No quarter was given.");
        _rejected = Assert.IsType<RejectedScenario>(CommissionEngine.Calculate(Scenario(quarter)));
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

    [Then("the statement list holds only {string}")]
    public void ThenTheStatementListHoldsOnly(string repIds) =>
        Assert.Equal(repIds.Split(',', StringSplitOptions.TrimEntries),
            (_result ?? throw new InvalidOperationException("The scenario was not calculated.")).Statements.Select(statement => statement.RepId));

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

    private ScenarioInput Scenario(QuarterPeriod quarter) =>
        new("feature", "Feature scenario", quarter, _roster, _deals,
            _bookingQuarter is null ? [] : [new BookingQuarterInput(_bookingQuarter, _bookingReps, _partners, _bookingDeals)]);

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
