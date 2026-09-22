namespace CommissionCalculator.Specs.Support;

internal static class Fixtures
{
    public static string ScenarioPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Scenarios", fileName);

    // spec.md is read from the committed repository (constitution v1.1.0 allows reading committed files).
    public static string SpecPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "specs", "001-commission-calculator", "spec.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("spec.md was not found above the test output directory.");
    }
}
