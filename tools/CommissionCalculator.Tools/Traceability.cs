using System.Reflection;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CommissionCalculator.Tools;

internal sealed record CiEvidence(string Job, string Check, IReadOnlyList<string> Requirements, string Result, string RunUrl);

internal sealed record ExplainedGap(string Requirement, string Category, string Reason, string? Note = null);

internal sealed record TraceInput(
    IReadOnlyList<string> Requirements,
    IReadOnlyDictionary<string, List<string>> Members,
    IReadOnlyDictionary<string, List<string>> Tests,
    IReadOnlyList<string> ToolingTests,
    IReadOnlyList<CiEvidence> Evidence,
    IReadOnlyList<ExplainedGap> Explained);

internal static partial class Traceability
{
    // The gaps this project accounts for (task T061). Every FR/SC without a test or a member is
    // still reported as a gap; an entry here only says why, so it is listed under "Explained".
    public static readonly IReadOnlyList<ExplainedGap> ExplainedGaps =
    [
        new("FR-019", "scope",
            "USD only; tax, currency conversion and multi-year terms are out of scope, so no member implements them."),
        new("SC-001", "evidence-only",
            "The seeded scenario files are compared with Appendix A by the seeded-scenario tests (T060); no single member implements the criterion."),
        new("SC-002", "evidence-only",
            "Every breakdown line cites a requirement, checked by the page tests and the seeded-scenario tests (T030, T060)."),
        new("SC-003", "evidence-only",
            "The seeded scenarios exercise all eight compensation rules, checked by the seeded-scenario rule checks (T060)."),
        new("SC-004", "CI evidence",
            "Verified by the smoke and offline-smoke CI jobs (T010, T035, T060b), not by a test."),
        new("SC-005", "manual evidence",
            "Keyboard-only check T033 (PR #5) and screen-reader check T034, confirmed by the maintainer at the Phase 7 gate (PR #10); both re-run at T064 and recorded on the Phase 9 pull request.",
            "FR-021's screen-reader clause is covered by the same manual check (T034); FR-021 itself has tests and members and is not a gap."),
    ];

    public static IReadOnlyList<string> ParseRequirements(string specMarkdown) =>
        RequirementId().Matches(specMarkdown).Select(match => match.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();

    public static (Dictionary<string, List<string>> Tests, List<string> ToolingTests) ParseTrx(string trxXml)
    {
        var tests = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var tooling = new List<string>();

        foreach (var unitTest in XDocument.Parse(trxXml).Descendants().Where(element => element.Name.LocalName == "UnitTest"))
        {
            var method = unitTest.Descendants().Single(element => element.Name.LocalName == "TestMethod");
            var name = $"{(string?)method.Attribute("className")}.{(string?)method.Attribute("name")}";

            var principle = Property(unitTest, "Principle").FirstOrDefault();
            if (principle is not null)
            {
                tooling.Add($"{principle}: {name}");
                continue;
            }

            var categories = unitTest.Descendants()
                .Where(element => element.Name.LocalName == "TestCategoryItem")
                .Select(item => (string?)item.Attribute("TestCategory"));

            foreach (var requirement in Property(unitTest, "Requirement").Concat(categories)
                         .Where(value => value is not null && BareRequirementId().IsMatch(value))
                         .Select(value => value!)
                         .Distinct(StringComparer.Ordinal))
            {
                tests.TryAdd(requirement, []);
                tests[requirement].Add(name);
            }
        }

        return (tests, tooling);
    }

    public static Dictionary<string, List<string>> ReadMembers(string assemblyPath)
    {
        var members = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        // GetTypes() throws ReflectionTypeLoadException when a shared framework the assembly needs
        // is missing; that failure must surface rather than be reported as an empty trace.
        foreach (var type in Assembly.LoadFrom(assemblyPath).GetTypes())
        {
            Add(type, type.FullName ?? type.Name);
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                                                   | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                Add(method, $"{type.FullName}.{method.Name}");
            }
        }

        return members;

        void Add(MemberInfo member, string name)
        {
            foreach (var requirement in CustomAttributeData.GetCustomAttributes(member)
                         .Where(attribute => attribute.AttributeType.FullName == "CommissionCalculator.Engine.ImplementsAttribute")
                         .Select(attribute => (string)attribute.ConstructorArguments[0].Value!))
            {
                members.TryAdd(requirement, []);
                if (!members[requirement].Contains(name, StringComparer.Ordinal))
                {
                    members[requirement].Add(name);
                }
            }
        }
    }

    private static readonly JsonSerializerOptions EvidenceJson = new() { PropertyNameCaseInsensitive = true };

    public static CiEvidence ParseEvidence(string json) =>
        JsonSerializer.Deserialize<CiEvidence>(json, EvidenceJson)
        ?? throw new InvalidOperationException("The CI-evidence record is empty.");

    public static string Report(TraceInput input)
    {
        var report = new StringBuilder()
            .AppendLine("# Traceability")
            .AppendLine()
            .AppendLine("Generated by `dotnet run --project tools/CommissionCalculator.Tools -- trace`; do not edit by hand.")
            .AppendLine()
            .AppendLine("## Requirements")
            .AppendLine()
            .AppendLine("| Requirement | Implemented by | Tests |")
            .AppendLine("| --- | --- | --- |");

        foreach (var requirement in input.Requirements)
        {
            report.AppendLine(CultureInfo.InvariantCulture, $"| {requirement} | {Code(Lookup(input.Members, requirement))} | {Code(Lookup(input.Tests, requirement))} |");
        }

        var gaps = input.Requirements
            .Select(requirement => (Requirement: requirement, Missing: Missing(input, requirement)))
            .Where(gap => gap.Missing.Length > 0)
            .Select(gap => (gap.Requirement, gap.Missing, Explanation: Explanation(input, gap.Requirement)))
            .ToList();

        report.AppendLine().AppendLine("## Gaps").AppendLine()
            .AppendLine("### Unexplained").AppendLine();
        var unexplained = gaps.Where(gap => gap.Explanation is null).ToList();
        if (unexplained.Count == 0)
        {
            report.AppendLine("None.");
        }
        else
        {
            report.AppendLine("| Requirement | Missing |").AppendLine("| --- | --- |");
            foreach (var gap in unexplained)
            {
                report.AppendLine(CultureInfo.InvariantCulture, $"| {gap.Requirement} | {gap.Missing} |");
            }
        }

        report.AppendLine().AppendLine("### Explained").AppendLine();
        var explained = gaps.Where(gap => gap.Explanation is not null).ToList();
        if (explained.Count == 0)
        {
            report.AppendLine("None.");
        }
        else
        {
            report.AppendLine("| Requirement | Missing | Category | Reason |").AppendLine("| --- | --- | --- | --- |");
            foreach (var gap in explained)
            {
                report.AppendLine(CultureInfo.InvariantCulture, $"| {gap.Requirement} | {gap.Missing} | {gap.Explanation!.Category} | {gap.Explanation.Reason} |");
            }

            foreach (var gap in explained.Where(gap => gap.Explanation!.Note is not null))
            {
                report.AppendLine().AppendLine(CultureInfo.InvariantCulture, $"Note on {gap.Requirement}: {gap.Explanation!.Note}");
            }
        }

        report.AppendLine().AppendLine("## Verified by CI job").AppendLine();
        if (input.Evidence.Count == 0)
        {
            report.AppendLine("No CI-evidence records were supplied.");
        }
        else
        {
            report.AppendLine("| Job | Check | Requirements | Result | Run |").AppendLine("| --- | --- | --- | --- | --- |");
            foreach (var record in input.Evidence)
            {
                report.AppendLine(CultureInfo.InvariantCulture, $"| {record.Job} | {record.Check} | {string.Join(", ", record.Requirements)} | {record.Result} | {record.RunUrl} |");
            }
        }

        report.AppendLine().AppendLine("## Tooling tests").AppendLine()
            .AppendLine("Tests of the project's own tooling; they carry a principle, not a requirement.").AppendLine()
            .AppendLine("| Principle | Test |").AppendLine("| --- | --- |");
        foreach (var test in input.ToolingTests)
        {
            var parts = test.Split(": ", 2);
            report.AppendLine(CultureInfo.InvariantCulture, $"| {parts[0]} | `{parts[1]}` |");
        }

        return report.ToString();
    }

    private static string Missing(TraceInput input, string requirement) => string.Join(", ",
        new[]
        {
            Lookup(input.Tests, requirement).Count == 0 ? "no test" : null,
            Lookup(input.Members, requirement).Count == 0 ? "no member" : null,
        }.Where(part => part is not null));

    // A CI-evidence record explains nothing while it is failing: the gap goes back to Unexplained.
    private static ExplainedGap? Explanation(TraceInput input, string requirement) =>
        input.Evidence.Any(record => record.Requirements.Contains(requirement, StringComparer.Ordinal)
                                     && !record.Result.Equals("success", StringComparison.OrdinalIgnoreCase))
            ? null
            : input.Explained.FirstOrDefault(gap => gap.Requirement == requirement);

    private static IEnumerable<string?> Property(XElement unitTest, string key) =>
        unitTest.Descendants().Where(element => element.Name.LocalName == "Property")
            .Where(property => (string?)property.Elements().FirstOrDefault(child => child.Name.LocalName == "Key") == key)
            .Select(property => (string?)property.Elements().FirstOrDefault(child => child.Name.LocalName == "Value"));

    private static List<string> Lookup(IReadOnlyDictionary<string, List<string>> source, string requirement) =>
        source.TryGetValue(requirement, out var found) ? found : [];

    // Name order, and one entry per name: a theory's cases share a method name, so the count says
    // how many of them cover the requirement. The report is committed, so it must not reorder.
    private static string Code(List<string> names) =>
        names.Count == 0
            ? "—"
            : string.Join(", ", names.GroupBy(name => name, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => group.Count() == 1 ? $"`{group.Key}`" : $"`{group.Key}` (x{group.Count()})"));

    [GeneratedRegex(@"\*\*((?:FR|SC)-\d{3})\*\*")]
    private static partial Regex RequirementId();

    [GeneratedRegex(@"^(?:FR|SC)-\d{3}$")]
    private static partial Regex BareRequirementId();
}
