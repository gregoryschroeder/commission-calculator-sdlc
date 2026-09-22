using CommissionCalculator.Engine;

namespace CommissionCalculator.Specs.Support;

internal sealed record SeedStatement(string ScenarioId, RepStatement Statement);

// The brief's eight compensation rules, each a check over the loaded seeded statements (SC-003).
// Each check fails when every scenario carrying its rule is removed, which the feature proves.
internal static class SeedRules
{
    public static bool Rule1QuotaPerRep(IReadOnlyList<SeedStatement> seeds) =>
        seeds.Select(seed => seed.Statement.Quota).Distinct().Count() > 1;

    private static readonly string[] RatePrefixes = ["5% of", "8% of", "12% of"];

    public static bool Rule2TieredRates(IReadOnlyList<SeedStatement> seeds) =>
        RatePrefixes.All(prefix =>
            Lines(seeds).Any(line => line.RequirementId == "FR-006" && line.Description.StartsWith(prefix, StringComparison.Ordinal)));

    public static bool Rule3ProratedQuota(IReadOnlyList<SeedStatement> seeds) =>
        Lines(seeds).Any(line => line.RequirementId == "FR-010");

    public static bool Rule4SplitDeals(IReadOnlyList<SeedStatement> seeds) =>
        Lines(seeds).Any(line => line.RequirementId == "FR-012");

    public static bool Rule5DrawRecoverable(IReadOnlyList<SeedStatement> seeds) =>
        seeds.Any(seed => seed.Statement.Recovered != 0m) && seeds.Any(seed => seed.Statement.Payable != 0m);

    public static bool Rule6Clawback(IReadOnlyList<SeedStatement> seeds) =>
        Lines(seeds).Any(line => line.Section == LineSection.Clawback && line.Amount != 0m);

    public static bool Rule7BookingAndCloseDates(IReadOnlyList<SeedStatement> seeds) =>
        Lines(seeds).Any(line => line.Section == LineSection.Excluded && line.RequirementId == "FR-008");

    // Rule 8 is the strict reader itself: a seed carrying a currency, tax or term field does not
    // load at all, and every amount the page shows is in dollars.
    public static bool Rule8UsDollarsOnly(IReadOnlyList<string> renderedAmounts) =>
        renderedAmounts.All(amount => amount.StartsWith('$') || amount.StartsWith("−$", StringComparison.Ordinal));

    public static bool Rule8UsDollarsOnly(IReadOnlyList<string> renderedAmounts, bool everySeedLoaded) =>
        everySeedLoaded && Rule8UsDollarsOnly(renderedAmounts);

    private static IEnumerable<BreakdownLine> Lines(IEnumerable<SeedStatement> seeds) =>
        seeds.SelectMany(seed => seed.Statement.Lines);
}
