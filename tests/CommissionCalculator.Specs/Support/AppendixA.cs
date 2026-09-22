using System.Globalization;
using System.Text.RegularExpressions;

namespace CommissionCalculator.Specs.Support;

internal sealed record ExpectedLine(string Item, decimal Amount, string Rule);

internal sealed record ExpectedStatement(string RepName, IReadOnlyList<ExpectedLine> Lines);

internal sealed record ExpectedScenario(string Id, IReadOnlyList<ExpectedStatement> Statements);

// Appendix A of the committed spec.md, parsed as the expected statements. The seeds are checked
// against the spec itself, not against a copy of it (constitution v1.1.0 allows reading committed
// files).
internal static partial class AppendixA
{
    public static IReadOnlyList<ExpectedScenario> Scenarios { get; } = Parse(File.ReadAllText(Fixtures.SpecPath()));

    public static ExpectedScenario For(string id) =>
        Scenarios.SingleOrDefault(scenario => scenario.Id == id)
        ?? throw new InvalidOperationException($"Appendix A has no scenario '{id}'.");

    private static List<ExpectedScenario> Parse(string spec)
    {
        var appendix = spec[spec.IndexOf("## Appendix A:", StringComparison.Ordinal)..];
        var headings = SectionHeading().Matches(appendix).Cast<Match>().ToList();
        return headings.Select((heading, index) =>
        {
            var end = index + 1 < headings.Count ? headings[index + 1].Index : appendix.Length;
            return new ExpectedScenario(heading.Groups[1].Value, ParseStatements(appendix[heading.Index..end]));
        }).ToList();
    }

    private static List<ExpectedStatement> ParseStatements(string section)
    {
        var statements = new List<ExpectedStatement>();
        foreach (Match heading in RepHeading().Matches(section))
        {
            var rest = section[(heading.Index + heading.Length)..];
            var next = RepHeading().Match(rest);
            var body = next.Success ? rest[..next.Index] : rest;
            var lines = LineRow().Matches(body)
                .Select(row => new ExpectedLine(row.Groups[1].Value.Trim(), Money(row.Groups[2].Value), row.Groups[3].Value.Trim()))
                .ToList();
            if (lines.Count > 0)
            {
                statements.Add(new ExpectedStatement(heading.Groups[1].Value, lines));
            }
        }

        return statements;
    }

    private static decimal Money(string text) =>
        decimal.Parse(text.Replace("−", "-", StringComparison.Ordinal).Replace("$", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal),
            NumberStyles.Number, CultureInfo.InvariantCulture);

    [GeneratedRegex(@"^### A\.\d+ `([a-z0-9-]+)`", RegexOptions.Multiline)]
    private static partial Regex SectionHeading();

    [GeneratedRegex(@"^\*\*([A-Za-z]+)\*\*$", RegexOptions.Multiline)]
    private static partial Regex RepHeading();

    [GeneratedRegex(@"^\| (.+?) \| (−?\$[\d,]+\.\d{2}) \| (FR-\d{3}) \|$", RegexOptions.Multiline)]
    private static partial Regex LineRow();
}
