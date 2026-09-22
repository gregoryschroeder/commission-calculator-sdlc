namespace CommissionCalculator.Web.Catalog;

public sealed class ScenarioOptions
{
    // The build/publish output, not the content root: a seed file missing from output is missing
    // at run time, which is what the smoke checks need to see (tasks T024).
    public string Directory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Scenarios");
}

[Engine.Implements("FR-001")]
public sealed class ScenarioCatalog
{
    public ScenarioCatalog(string directory)
    {
        if (!System.IO.Directory.Exists(directory))
        {
            return;
        }

        var results = System.IO.Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path => ScenarioFileReader.Read(Path.GetFileName(path), File.ReadAllText(path)))
            .ToList();

        var read = results.OfType<ScenarioRead>().Select(result => result.File).ToList();
        var duplicateIds = read.GroupBy(file => file.Scenario.Id)
            .Where(group => group.Count() > 1)
            .ToDictionary(group => group.Key, group => group.Select(file => file.FileName).ToList());

        Scenarios = read.Where(file => !duplicateIds.ContainsKey(file.Scenario.Id)).ToList();
        LoadErrors = results.OfType<ScenarioReadFailed>().Select(result => result.Error)
            .Concat(read.Where(file => duplicateIds.ContainsKey(file.Scenario.Id))
                .Select(file => new ScenarioLoadError(file.FileName,
                    $"Scenario id '{file.Scenario.Id}' is declared by more than one file: {string.Join(", ", duplicateIds[file.Scenario.Id])}.")))
            .OrderBy(error => error.FileName, StringComparer.Ordinal)
            .ToList();
    }

    public IReadOnlyList<ScenarioFile> Scenarios { get; } = [];

    public IReadOnlyList<ScenarioLoadError> LoadErrors { get; } = [];
}
