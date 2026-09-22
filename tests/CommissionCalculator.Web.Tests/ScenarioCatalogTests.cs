using CommissionCalculator.Web.Catalog;
using Xunit;

namespace CommissionCalculator.Web.Tests;

public sealed class ScenarioCatalogTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "catalog-tests-" + Guid.NewGuid().ToString("N"));

    public ScenarioCatalogTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private void Write(string fileName, string json) => File.WriteAllText(Path.Combine(_directory, fileName), json);

    [Fact, Trait("Requirement", "FR-001")]
    public void ScenariosAreListedInFileNameOrder()
    {
        Write("b.json", ScenarioJson.WithId("second"));
        Write("a.json", ScenarioJson.WithId("first"));

        var catalog = new ScenarioCatalog(_directory);

        Assert.Equal(["first", "second"], catalog.Scenarios.Select(file => file.Scenario.Id));
    }

    [Fact, Trait("Requirement", "FR-001")]
    public void AFileThatFailsToLoadDoesNotHideTheOthers()
    {
        Write("a.json", "{ not json");
        Write("b.json", ScenarioJson.WithId("second"));

        var catalog = new ScenarioCatalog(_directory);

        Assert.Equal(["second"], catalog.Scenarios.Select(file => file.Scenario.Id));
        Assert.Equal(["a.json"], catalog.LoadErrors.Select(error => error.FileName));
    }

    [Fact, Trait("Requirement", "FR-004")]
    public void TwoFilesWithTheSameScenarioIdAreBothLoadErrors()
    {
        Write("a.json", ScenarioJson.WithId("same"));
        Write("b.json", ScenarioJson.WithId("same"));

        var catalog = new ScenarioCatalog(_directory);

        Assert.Empty(catalog.Scenarios);
        Assert.Equal(["a.json", "b.json"], catalog.LoadErrors.Select(error => error.FileName));
        Assert.All(catalog.LoadErrors, error => Assert.Contains("same", error.Message, StringComparison.Ordinal));
    }

    [Fact, Trait("Requirement", "FR-001")]
    public void AMissingDirectoryIsAnEmptyCatalog()
    {
        var catalog = new ScenarioCatalog(Path.Combine(_directory, "does-not-exist"));

        Assert.Empty(catalog.Scenarios);
        Assert.Empty(catalog.LoadErrors);
    }
}
