using Xunit;

namespace CommissionCalculator.Tools.Tests;

[Trait("Principle", "II")]
public sealed class CoverageGateTests
{
    private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    [Fact]
    public void EngineRateAboveTheFloorPasses()
    {
        var result = CoverageGate.Evaluate([Fixture("engine-85.cobertura.xml")], engineHasTypes: true, floorPercent: 80m);

        Assert.True(result.Passed);
        Assert.Contains("85.00%", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EngineRateBelowTheFloorFails()
    {
        var result = CoverageGate.Evaluate([Fixture("engine-79.cobertura.xml")], engineHasTypes: true, floorPercent: 80m);

        Assert.False(result.Passed);
        Assert.Contains("79.00%", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NoEnginePackageFailsWhenTheEngineHasTypes()
    {
        var result = CoverageGate.Evaluate([Fixture("no-engine.cobertura.xml")], engineHasTypes: true, floorPercent: 80m);

        Assert.False(result.Passed);
    }

    [Fact]
    public void NoEnginePackagePassesWhenTheEngineHasNoTypes()
    {
        var result = CoverageGate.Evaluate([Fixture("no-engine.cobertura.xml")], engineHasTypes: false, floorPercent: 80m);

        Assert.True(result.Passed);
        Assert.Contains("no engine lines yet", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsAreMergedAsAUnionOfCoveredLines()
    {
        var result = CoverageGate.Evaluate(
            [Fixture("merge-a.cobertura.xml"), Fixture("no-engine.cobertura.xml"), Fixture("merge-b.cobertura.xml")],
            engineHasTypes: true,
            floorPercent: 80m);

        Assert.True(result.Passed);
        Assert.Contains("90.00%", result.Message, StringComparison.Ordinal);
    }
}
