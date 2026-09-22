using System.Text.RegularExpressions;
using CommissionCalculator.Engine;
using CommissionCalculator.Specs.Support;
using CommissionCalculator.Web.Catalog;
using CommissionCalculator.Web.Presentation;
using Reqnroll;
using Xunit;

namespace CommissionCalculator.Specs.Steps;

// The shipped seed files, computed and compared with Appendix A of the committed spec.
[Binding]
public sealed partial class SeededScenarioSteps
{
    private readonly List<SeedStatement> _seeds = [];
    private readonly List<string> _renderedAmounts = [];
    private string? _scenarioId;
    private RejectedScenario? _rejected;
    private bool _everySeedLoaded = true;

    [Given("the shipped scenario file {string}")]
    public void GivenTheShippedScenarioFile(string id)
    {
        _scenarioId = id;
        Load([id], quota: null, currencyIn: null);
    }

    [Given("all the shipped scenario files")]
    public void GivenAllTheShippedScenarioFiles() => Load(SeedFiles.Ids, quota: null, currencyIn: null);

    [Given("all the shipped scenario files except {string}")]
    public void GivenAllTheShippedScenarioFilesExcept(string removed)
    {
        var excluded = removed.Split(',', StringSplitOptions.TrimEntries).ToHashSet(StringComparer.Ordinal);
        Load([.. SeedFiles.Ids.Where(id => !excluded.Contains(id))], quota: null, currencyIn: null);
    }

    [Given("all the shipped scenario files with every quota set to ${decimal}")]
    public void GivenEveryQuotaSetTo(decimal quota) => Load(SeedFiles.Ids, quota, currencyIn: null);

    [Given("all the shipped scenario files with a currency field added to one of them")]
    public void GivenACurrencyFieldAddedToOne() => Load(SeedFiles.Ids, quota: null, currencyIn: "tiers");

    [Then("every statement matches Appendix A line for line")]
    public void ThenEveryStatementMatchesAppendixA()
    {
        var expected = AppendixA.For(ScenarioId);
        Assert.Equal(expected.Statements.Count, _seeds.Count);
        foreach (var (statement, expectedStatement) in _seeds.Select(seed => seed.Statement).Zip(expected.Statements))
        {
            Assert.Equal(expectedStatement.RepName, statement.Name);
            Assert.Equal(
                expectedStatement.Lines.Select(line => (line.Item, line.Amount, line.Rule)),
                statement.Lines.Select(line => (line.Description, line.Amount, line.RequirementId)));
        }
    }

    [Then("every summary field matches its Appendix A line")]
    public void ThenEverySummaryFieldMatchesItsLine()
    {
        foreach (var (statement, expected) in _seeds.Select(seed => seed.Statement).Zip(AppendixA.For(ScenarioId).Statements))
        {
            decimal Line(string item) => expected.Lines.Last(line => line.Item.StartsWith(item, StringComparison.Ordinal)).Amount;

            Assert.Equal(Line("Quarterly quota"), statement.Quota);
            Assert.Equal(expected.Lines.Any(line => line.Item.StartsWith("Prorated quota", StringComparison.Ordinal))
                ? Line("Prorated quota")
                : Line("Quarterly quota"), statement.ProratedQuota);
            Assert.Equal(Line("Credited bookings"), statement.CreditedBookings);
            Assert.Equal(Line("Commission before refunds"), statement.CommissionBeforeRefunds);
            Assert.Equal(Line("Clawbacks"), statement.Clawbacks);
            Assert.Equal(Line("Earned commission"), statement.EarnedCommission);
            Assert.Equal(Line("Draws paid"), statement.DrawPaid);
            Assert.Equal(Line("Draw recovered"), statement.Recovered);
            Assert.Equal(Line("Commission payable"), statement.Payable);
            Assert.Equal(Line("Closing recoverable balance"), statement.ClosingRecoverableBalance);
        }
    }

    [Then("every breakdown line cites a requirement that exists in spec.md")]
    public void ThenEveryLineCitesARequirementInTheSpec()
    {
        var requirements = SpecRequirement().Matches(File.ReadAllText(Fixtures.SpecPath()))
            .Select(match => match.Groups[1].Value).ToHashSet();
        var cited = _seeds.SelectMany(seed => seed.Statement.Lines).Select(line => line.RequirementId).ToList();

        Assert.NotEmpty(cited);
        Assert.All(cited, requirement => Assert.Contains(requirement, requirements));
    }

    [Then("it is rejected with exactly {int} errors citing {string}")]
    public void ThenItIsRejectedWith(int count, string requirements)
    {
        var rejected = _rejected ?? throw new InvalidOperationException("The scenario was not rejected.");

        Assert.Equal(count, rejected.Errors.Count);
        Assert.Equal(
            requirements.Split(',', StringSplitOptions.TrimEntries).Order(StringComparer.Ordinal),
            rejected.Errors.Select(error => error.RequirementId).Order(StringComparer.Ordinal));
    }

    [Then("rule {int} holds")]
    public void ThenRuleHolds(int rule) => Assert.True(Rule(rule), $"rule {rule}");

    [Then("rule {int} does not hold")]
    public void ThenRuleDoesNotHold(int rule) => Assert.False(Rule(rule), $"rule {rule}");

    [Then("the dollar check rejects {string}")]
    public static void ThenTheDollarCheckRejects(string amount) => Assert.False(SeedRules.Rule8UsDollarsOnly([amount]));

    private bool Rule(int rule) => rule switch
    {
        1 => SeedRules.Rule1QuotaPerRep(_seeds),
        2 => SeedRules.Rule2TieredRates(_seeds),
        3 => SeedRules.Rule3ProratedQuota(_seeds),
        4 => SeedRules.Rule4SplitDeals(_seeds),
        5 => SeedRules.Rule5DrawRecoverable(_seeds),
        6 => SeedRules.Rule6Clawback(_seeds),
        7 => SeedRules.Rule7BookingAndCloseDates(_seeds),
        8 => SeedRules.Rule8UsDollarsOnly(_renderedAmounts, _everySeedLoaded) && _seeds.Count > 0,
        _ => throw new ArgumentOutOfRangeException(nameof(rule)),
    };

    private string ScenarioId => _scenarioId ?? throw new InvalidOperationException("No scenario was loaded.");

    private void Load(IReadOnlyList<string> ids, decimal? quota, string? currencyIn)
    {
        foreach (var id in ids)
        {
            var json = SeedFiles.Read(id);
            if (id == currencyIn)
            {
                json = json.Replace("\"id\":", "\"currency\": \"EUR\",\n  \"id\":", StringComparison.Ordinal);
            }

            var read = ScenarioFileReader.Read(id + ".json", json);
            if (read is not ScenarioRead loaded)
            {
                _everySeedLoaded = false;
                continue;
            }

            var scenario = quota is null
                ? loaded.File.Scenario
                : loaded.File.Scenario with { Roster = [.. loaded.File.Scenario.Roster.Select(rep => rep with { Quota = quota.Value })] };

            switch (CommissionEngine.Calculate(scenario))
            {
                case CalculatedScenario calculated:
                    _seeds.AddRange(calculated.Statements.Select(statement => new SeedStatement(id, statement)));
                    _renderedAmounts.AddRange(calculated.Statements.SelectMany(statement =>
                        statement.Lines.Select(line => Format.Money(line.Amount))));
                    break;
                case RejectedScenario rejected:
                    _rejected = rejected;
                    break;
            }
        }
    }

    [GeneratedRegex(@"\*\*(FR-\d{3})\*\*")]
    private static partial Regex SpecRequirement();
}
