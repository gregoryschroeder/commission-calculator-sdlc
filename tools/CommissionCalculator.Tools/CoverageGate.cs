using System.Globalization;
using System.Xml.Linq;

namespace CommissionCalculator.Tools;

internal sealed record GateResult(bool Passed, string Message);

internal static class CoverageGate
{
    public const string EnginePackage = "CommissionCalculator.Engine";

    // One report is written per test project; a line counts as covered if any report covers it.
    public static GateResult Evaluate(IReadOnlyList<string> coberturaReports, bool engineHasTypes, decimal floorPercent)
    {
        var lines = EngineLines(coberturaReports);
        if (lines.Count == 0)
        {
            return engineHasTypes
                ? new GateResult(false, $"No coverage for {EnginePackage} was reported, but the engine has types.")
                : new GateResult(true, "Engine coverage: no engine lines yet.");
        }

        var percent = 100m * lines.Count(line => line.Value) / lines.Count;
        var passed = percent >= floorPercent;
        var rate = percent.ToString("0.00", CultureInfo.InvariantCulture);
        var floor = floorPercent.ToString("0.##", CultureInfo.InvariantCulture);
        return new GateResult(passed, $"Engine line coverage: {rate}% (floor {floor}%) — {(passed ? "pass" : "FAIL")}.");
    }

    private static Dictionary<(string File, int Line), bool> EngineLines(IEnumerable<string> reports)
    {
        var lines = new Dictionary<(string, int), bool>();
        foreach (var report in reports)
        {
            var enginePackages = XDocument.Parse(report).Descendants("package")
                .Where(package => (string?)package.Attribute("name") == EnginePackage);
            foreach (var @class in enginePackages.Descendants("class"))
            {
                var file = (string?)@class.Attribute("filename") ?? "";
                foreach (var line in @class.Descendants("line"))
                {
                    var key = (file, (int)line.Attribute("number")!);
                    var covered = (int)line.Attribute("hits")! > 0;
                    lines[key] = lines.GetValueOrDefault(key) || covered;
                }
            }
        }

        return lines;
    }
}
