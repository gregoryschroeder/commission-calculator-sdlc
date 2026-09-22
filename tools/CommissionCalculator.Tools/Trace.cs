namespace CommissionCalculator.Tools;

// The `trace` command: reads the spec, the built assemblies, the test results and any CI-evidence
// records, and writes the traceability report (task T061).
internal static class Trace
{
    private const string DefaultSpec = "specs/001-commission-calculator/spec.md";
    private const string DefaultOut = "specs/001-commission-calculator/traceability.md";

    public static int Run(IReadOnlyList<string> options)
    {
        var (spec, assemblies, results, evidence, output, strict) = Parse(options);
        if (assemblies.Count == 0 || results.Count == 0)
        {
            Console.Error.WriteLine("trace: --assemblies and --results are both required.");
            return 2;
        }

        var members = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var (requirement, names) in assemblies.SelectMany(Traceability.ReadMembers))
        {
            members.TryAdd(requirement, []);
            members[requirement].AddRange(names.Except(members[requirement], StringComparer.Ordinal));
        }

        var tests = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var tooling = new List<string>();
        foreach (var (suiteTests, suiteTooling) in results.Select(path => Traceability.ParseTrx(File.ReadAllText(path))))
        {
            foreach (var (requirement, names) in suiteTests)
            {
                tests.TryAdd(requirement, []);
                tests[requirement].AddRange(names);
            }

            tooling.AddRange(suiteTooling);
        }

        var report = Traceability.Report(new TraceInput(
            Traceability.ParseRequirements(File.ReadAllText(spec)),
            members,
            tests,
            [.. tooling.Order(StringComparer.Ordinal)],
            [.. evidence.Select(path => Traceability.ParseEvidence(File.ReadAllText(path))).OrderBy(record => record.Job, StringComparer.Ordinal)],
            Traceability.ExplainedGaps));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        File.WriteAllText(output, report);

        var unexplained = Unexplained(report);
        Console.WriteLine($"trace: wrote {output}; {unexplained} unexplained gap(s).");
        return strict && unexplained > 0 ? 1 : 0;
    }

    private static int Unexplained(string report)
    {
        var start = report.IndexOf("### Unexplained", StringComparison.Ordinal);
        var end = report.IndexOf("### Explained", StringComparison.Ordinal);
        return report[start..end].Split('\n').Count(line => line.StartsWith("| FR-", StringComparison.Ordinal)
                                                           || line.StartsWith("| SC-", StringComparison.Ordinal));
    }

    private static (string Spec, List<string> Assemblies, List<string> Results, List<string> Evidence, string Out, bool Strict)
        Parse(IReadOnlyList<string> options)
    {
        var (spec, output, strict) = (DefaultSpec, DefaultOut, false);
        var (assemblies, results, evidence) = (new List<string>(), new List<string>(), new List<string>());
        List<string>? current = null;

        for (var index = 0; index < options.Count; index++)
        {
            switch (options[index])
            {
                case "--spec":
                    (spec, current) = (options[++index], null);
                    break;
                case "--out":
                    (output, current) = (options[++index], null);
                    break;
                case "--strict":
                    (strict, current) = (true, null);
                    break;
                case "--assemblies":
                    current = assemblies;
                    break;
                case "--results":
                    current = results;
                    break;
                case "--evidence":
                    current = evidence;
                    break;
                default:
                    current?.Add(options[index]);
                    break;
            }
        }

        return (spec, assemblies, results, evidence, output, strict);
    }
}
