using System.Diagnostics;
using Xunit;

namespace CommissionCalculator.Tools.Tests;

// The traceability generator (task T061, Principle III): every FR/SC either has a test and a
// member, or is listed as a gap.
[Trait("Principle", "III")]
public sealed class TraceabilityTests
{
    private static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static string Fixture(string name) => File.ReadAllText(FixturePath(name));

    [Fact]
    public void RequirementsAreReadFromTheSpecInOrderAndOnlyOnce()
    {
        var requirements = Traceability.ParseRequirements(Fixture("trace-spec.md"));

        Assert.Equal(["FR-001", "FR-002", "FR-003", "SC-001"], requirements);
    }

    [Fact]
    public void RequirementTraitsAndScenarioCategoriesBothMapTestsToRequirements()
    {
        var (tests, _) = Traceability.ParseTrx(Fixture("trace.trx"));

        Assert.Equal(["Fixture.EngineTests.CreditsTheDeal", "Fixture.Features.SomeFeature.FR003IsCoveredByAScenario"],
            tests["FR-001"]);
        Assert.Equal(["Fixture.Features.SomeFeature.FR003IsCoveredByAScenario"], tests["FR-003"]);
    }

    [Fact]
    public void ATestWithAPrincipleTraitIsAToolingTestAndCountsForNoRequirement()
    {
        var (tests, tooling) = Traceability.ParseTrx(Fixture("trace.trx"));

        Assert.Equal(["III: Fixture.ToolsTests.TheGateFails"], tooling);
        Assert.DoesNotContain("FR-002", tests.Keys);
    }

    [Fact]
    public void ImplementsAttributesAreReadByTheirFullTypeName()
    {
        var members = Traceability.ReadMembers(typeof(TraceabilityTests).Assembly.Location);

        Assert.Equal(["CommissionCalculator.Tools.Tests.TracedFixture"], members["FR-001"]);
        Assert.Equal(["CommissionCalculator.Tools.Tests.TracedFixture.TracedMethod"], members["FR-002"]);
    }

    [Fact]
    public void ARequirementWithBothAMemberAndATestIsNotAGap()
    {
        var report = Traceability.Report(Input());

        Assert.Contains("| FR-001 | `CommissionCalculator.Tools.Tests.TracedFixture` |", report, StringComparison.Ordinal);
        Assert.DoesNotContain("FR-001", Section(report, "### Unexplained"), StringComparison.Ordinal);
        Assert.DoesNotContain("FR-001", Section(report, "### Explained"), StringComparison.Ordinal);
    }

    [Fact]
    public void ARequirementWithNoTestIsAnUnexplainedGap() =>
        Assert.Contains("| FR-002 | no test |", Section(Traceability.Report(Input()), "### Unexplained"), StringComparison.Ordinal);

    [Fact]
    public void ARequirementWithNoMemberIsAnUnexplainedGap() =>
        Assert.Contains("| FR-003 | no member |", Section(Traceability.Report(Input()), "### Unexplained"), StringComparison.Ordinal);

    [Fact]
    public void AGapInTheExplainedTableIsStillListedButWithItsCategoryAndReason()
    {
        var explained = Section(
            Traceability.Report(Input(explained: [new ExplainedGap("SC-001", "scope", "out of scope for this release.")])),
            "### Explained");

        Assert.Contains("| SC-001 | no test, no member | scope | out of scope for this release. |", explained, StringComparison.Ordinal);
        Assert.DoesNotContain("SC-001", Section(Traceability.Report(
            Input(explained: [new ExplainedGap("SC-001", "scope", "out of scope for this release.")])), "### Unexplained"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void APassingCiRecordIsListedAsEvidenceAndIsNeverCountedAsATest()
    {
        var report = Traceability.Report(Input(
            evidence: [Traceability.ParseEvidence(Fixture("trace-evidence.json"))],
            explained: [new ExplainedGap("SC-001", "CI evidence", "the smoke job.")]));

        Assert.Contains("| smoke | clean checkout starts with one command and serves / | SC-001 | success |",
            Section(report, "## Verified by CI job"), StringComparison.Ordinal);
        Assert.Contains("| SC-001 | no test, no member |", Section(report, "### Explained"), StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedCiRecordLeavesItsGapUnexplained()
    {
        var failed = Traceability.ParseEvidence(Fixture("trace-evidence.json")) with { Result = "failure" };

        var report = Traceability.Report(Input(
            evidence: [failed],
            explained: [new ExplainedGap("SC-001", "CI evidence", "the smoke job.")]));

        Assert.Contains("| SC-001 | no test, no member |", Section(report, "### Unexplained"), StringComparison.Ordinal);
        Assert.DoesNotContain("SC-001", Section(report, "### Explained"), StringComparison.Ordinal);
    }

    // The web assembly's types need the ASP.NET shared framework, which the test host does not
    // have; the tools executable does, so the trace runs as its own process (research R8).
    [Fact]
    public void TheToolsExecutableTracesARazorPagesAssemblyInASeparateProcess()
    {
        var output = Path.Combine(Path.GetTempPath(), $"traceability-{Guid.NewGuid():N}.md");
        var (exitCode, console) = RunTools(
            "trace",
            "--spec", FixturePath("trace-spec.md"),
            "--assemblies", FixtureAssembly(),
            "--results", FixturePath("trace.trx"),
            "--out", output);

        Assert.True(exitCode == 0, console);
        Assert.Contains("CommissionCalculator.Tools.Fixture.Pages.FixturePageModel", File.ReadAllText(output), StringComparison.Ordinal);
        File.Delete(output);
    }

    private static TraceInput Input(IReadOnlyList<CiEvidence>? evidence = null, IReadOnlyList<ExplainedGap>? explained = null)
    {
        var (tests, tooling) = Traceability.ParseTrx(Fixture("trace.trx"));
        return new TraceInput(
            Traceability.ParseRequirements(Fixture("trace-spec.md")),
            Traceability.ReadMembers(typeof(TraceabilityTests).Assembly.Location),
            tests,
            tooling,
            evidence ?? [],
            explained ?? []);
    }

    private static string Section(string report, string heading)
    {
        var start = report.IndexOf(heading, StringComparison.Ordinal);
        Assert.True(start >= 0, $"the report has no {heading} section:\n{report}");

        var next = report.IndexOf("\n#", start + heading.Length, StringComparison.Ordinal);
        return next < 0 ? report[start..] : report[start..next];
    }

    private static string FixtureAssembly() =>
        ProjectOutput("CommissionCalculator.Tools.Fixture");

    private static string ToolsAssembly() => ProjectOutput("CommissionCalculator.Tools");

    // The tools executable is run from its own output, where its runtimeconfig.json names the
    // shared frameworks it needs; the copy beside the tests has none.
    private static string ProjectOutput(string project)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "tools", project, "bin", Configuration, "net10.0", project + ".dll");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"{project}.dll was not found; Tools.Tests builds it by project reference.");
    }

    private static (int ExitCode, string Console) RunTools(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add(ToolsAssembly());
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        var console = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, console);
    }

    private static string Configuration =>
        AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            ? "Release"
            : "Debug";
}
