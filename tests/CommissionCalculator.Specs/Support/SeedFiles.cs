namespace CommissionCalculator.Specs.Support;

// The shipped seed files, read from the committed web project (tasks T060/T060a).
internal static class SeedFiles
{
    public static readonly IReadOnlyList<string> Ids =
    [
        "tiers", "booking-dates", "proration", "proration-q2", "splits", "split-rounding",
        "draw", "refunds", "refund-splits", "refunds-q2", "invalid",
    ];

    public static string Read(string id) => File.ReadAllText(Path.Combine(Directory(), id + ".json"));

    private static string Directory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "CommissionCalculator.Web", "Scenarios");
            if (System.IO.Directory.Exists(Path.GetDirectoryName(candidate)!))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("The web project was not found above the test output directory.");
    }
}
