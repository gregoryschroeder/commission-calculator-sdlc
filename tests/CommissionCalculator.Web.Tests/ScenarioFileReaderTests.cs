using CommissionCalculator.Engine;
using CommissionCalculator.Web.Catalog;
using Xunit;

namespace CommissionCalculator.Web.Tests;

public sealed class ScenarioFileReaderTests
{
    [Fact, Trait("Requirement", "FR-001")]
    public void AFileMatchingTheContractMapsToAScenarioInput()
    {
        var expected = new ScenarioInput(
            "tiers",
            "Tiered rates",
            new QuarterPeriod(new(2026, 1, 1), new(2026, 3, 31)),
            [new RepInput("avery", "Avery", 100_000.00m, new(2025, 6, 1), 0.00m)],
            [new DealInput("T-1", 80_000.00m, new(2026, 2, 10), new(2026, 2, 12), [new SplitCredit("avery", 100m)], [])],
            []);

        var read = Assert.IsType<ScenarioRead>(ScenarioFileReader.Read("tiers.json", ScenarioJson.Tiers));

        Assert.Equal("tiers.json", read.File.FileName);
        Assert.StartsWith("One rep per tier example", read.File.Description, StringComparison.Ordinal);
        Assert.Equivalent(expected, read.File.Scenario, strict: true);
    }

    [Fact, Trait("Requirement", "FR-004")]
    public void SubCentAmountsAreReadExactly()
    {
        var json = ScenarioJson.Tiers.Replace("\"amount\": 80000.00", "\"amount\": 10.005", StringComparison.Ordinal);

        var read = Assert.IsType<ScenarioRead>(ScenarioFileReader.Read("tiers.json", json));

        Assert.Equal(10.005m, read.File.Scenario.Deals[0].Amount);
    }

    [Theory, Trait("Requirement", "FR-004")]
    [InlineData("unknown property", "\"id\": \"tiers\",", "\"id\": \"tiers\", \"currency\": \"EUR\",")]
    [InlineData("malformed JSON", "\"bookingQuarters\": []", "\"bookingQuarters\": [")]
    [InlineData("missing required field", "\"name\": \"Tiered rates\",", "")]
    public void AnUnreadableFileIsALoadErrorNamingTheFile(string reason, string find, string replace)
    {
        var json = ScenarioJson.Tiers.Replace(find, replace, StringComparison.Ordinal);
        Assert.NotEqual(ScenarioJson.Tiers, json);

        var failed = Assert.IsType<ScenarioReadFailed>(ScenarioFileReader.Read("broken.json", json));

        Assert.Equal("broken.json", failed.Error.FileName);
        Assert.False(string.IsNullOrWhiteSpace(failed.Error.Message), reason);
    }
}
