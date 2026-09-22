using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CommissionCalculator.Engine;
using CommissionCalculator.Specs.Support;
using CommissionCalculator.Web.Catalog;
using CommissionCalculator.Web.Presentation;
using Reqnroll;
using Xunit;

namespace CommissionCalculator.Specs.Steps;

[Binding]
public sealed partial class PageSteps(AppHost host)
{
    private HttpClient? _client;
    private HttpResponseMessage? _response;
    private IHtmlDocument? _document;

    [Given("the application is running")]
    public void GivenTheApplicationIsRunning() => _client = host.Start();

    [Given("a scenario folder holding {string}")]
    public void GivenAScenarioFolderHolding(string fileName) => StartWith([fileName]);

    [Given("a scenario folder holding {string} and {string}")]
    public void GivenAScenarioFolderHoldingTwo(string first, string second) => StartWith([first, second]);

    [Given("an empty scenario folder")]
    public void GivenAnEmptyScenarioFolder() => StartWith([]);

    [Given("a scenario folder holding {string} and a malformed {string}")]
    public void GivenAScenarioFolderWithAMalformedFile(string fileName, string malformed)
    {
        var directory = StartWith([fileName]);
        File.WriteAllText(Path.Combine(directory, malformed), "{ not json");
    }

    [When("the home page is requested")]
    [When("the page is requested with no scenario")]
    public Task WhenTheHomePageIsRequested() => Request("/");

    [When("the page for scenario {string} is requested")]
    public Task WhenThePageForScenarioIsRequested(string id) => Request($"/?scenario={Uri.EscapeDataString(id)}");

    [Then("the response status is {int}")]
    public void ThenTheResponseStatusIs(int status) => Assert.Equal((HttpStatusCode)status, Response.StatusCode);

    [Then("the page language is {string}")]
    public void ThenThePageLanguageIs(string language) =>
        Assert.Equal(language, Document.DocumentElement.GetAttribute("lang"));

    [Then("the page has exactly one main landmark")]
    public void ThenThePageHasExactlyOneMainLandmark() => Assert.Single(Document.QuerySelectorAll("main"));

    [Then("the page has exactly one h1")]
    public void ThenThePageHasExactlyOneH1() => Assert.Single(Document.QuerySelectorAll("h1"));

    [Then("the skip link targets the main landmark")]
    public void ThenTheSkipLinkTargetsTheMainLandmark()
    {
        var main = Assert.Single(Document.QuerySelectorAll("main"));
        Assert.False(string.IsNullOrEmpty(main.Id), "The main landmark has no id to target.");
        Assert.Equal("#" + main.Id, Document.QuerySelector("a.skip-link")?.GetAttribute("href"));
    }

    [Then("the picker is a GET form with a labelled select named {string} and a submit button")]
    public void ThenThePickerIsAGetForm(string name)
    {
        var form = Picker;
        Assert.Equal("get", form.GetAttribute("method"), ignoreCase: true);
        var select = Assert.Single(form.QuerySelectorAll($"select[name='{name}']"));
        Assert.False(string.IsNullOrEmpty(select.Id));
        Assert.Single(form.QuerySelectorAll($"label[for='{select.Id}']"));
        Assert.Single(form.QuerySelectorAll("button[type='submit']"));
    }

    [Then("the picker offers the scenarios {string}")]
    public void ThenThePickerOffers(string ids) =>
        Assert.Equal(List(ids), Picker.QuerySelectorAll("option").Select(option => option.GetAttribute("value")));

    [Then("the picker offers no scenarios")]
    public void ThenThePickerOffersNoScenarios() => Assert.Empty(Picker.QuerySelectorAll("option"));

    [Then("there are exactly {int} rep sections, each labelled by its own h2")]
    public void ThenThereAreRepSections(int count)
    {
        var sections = RepSections;
        Assert.Equal(count, sections.Count);
        Assert.All(sections, section =>
        {
            var heading = Assert.Single(section.QuerySelectorAll("h2"));
            Assert.False(string.IsNullOrEmpty(heading.Id));
            Assert.Equal(heading.Id, section.GetAttribute("aria-labelledby"));
        });
    }

    [Then("every rep table has a caption and the column headers {string}")]
    public void ThenEveryRepTableHasCaptionAndHeaders(string headers)
    {
        Assert.NotEmpty(RepSections);
        Assert.All(RepSections, section =>
        {
            var table = Assert.Single(section.QuerySelectorAll("table"));
            Assert.False(string.IsNullOrWhiteSpace(table.QuerySelector("caption")?.TextContent));
            var columns = table.QuerySelectorAll("thead th");
            Assert.All(columns, column => Assert.Equal("col", column.GetAttribute("scope")));
            Assert.Equal(List(headers), columns.Select(column => column.TextContent.Trim()));
        });
    }

    [Then("the page has at least one Rule cell and every Rule cell is an FR in spec.md")]
    public void ThenEveryRuleCellIsAnFrInTheSpec()
    {
        var requirements = SpecRequirement().Matches(File.ReadAllText(Fixtures.SpecPath())).Select(match => match.Groups[1].Value).ToHashSet();
        var rules = RepSections.SelectMany(section => section.QuerySelectorAll("tbody tr"))
            .Select(row => row.QuerySelectorAll("td")[2].TextContent.Trim()).ToList();

        Assert.NotEmpty(rules);
        Assert.All(rules, rule => Assert.Contains(rule, requirements));
    }

    [Then("the page title is {string}")]
    public void ThenThePageTitleIs(string title) => Assert.Equal(title, Document.Title);

    [Then("every rep summary equals the engine's statement for {string}")]
    public void ThenEveryRepSummaryEqualsTheEngine(string fileName)
    {
        var statements = EngineStatements(fileName);
        Assert.Equal(statements.Count, RepSections.Count);
        foreach (var (statement, section) in statements.Zip(RepSections))
        {
            var summary = section.QuerySelectorAll("dl dt").ToDictionary(term => term.TextContent.Trim(), term => term.NextElementSibling!.TextContent.Trim());
            Assert.Equal(new Dictionary<string, string>
            {
                ["Quota"] = Format.Money(statement.Quota),
                ["Prorated quota"] = Format.Money(statement.ProratedQuota),
                ["Credited bookings"] = Format.Money(statement.CreditedBookings),
                ["Attainment"] = Format.Attainment(statement.Attainment),
                ["Earned commission"] = Format.Money(statement.EarnedCommission),
                ["Clawbacks"] = Format.Money(statement.Clawbacks),
                ["Draws paid"] = Format.Money(statement.DrawPaid),
                ["Draw recovered"] = Format.Money(statement.Recovered),
                ["Commission payable"] = Format.Money(statement.Payable),
                ["Closing recoverable balance"] = Format.Money(statement.ClosingRecoverableBalance),
            }, summary);
        }
    }

    [Then("the summary for {string} shows earned commission {string}")]
    public void ThenTheSummaryShowsEarnedCommission(string name, string amount) =>
        Assert.Equal(amount, SummaryValue(name, "Earned commission"));

    [Then("the summary for {string} shows attainment {string}")]
    public void ThenTheSummaryShowsAttainment(string name, string attainment)
    {
        var section = RepSections.Single(candidate => candidate.QuerySelector("h2")!.TextContent.Trim() == name);
        var term = section.QuerySelectorAll("dl dt").Single(candidate => candidate.TextContent.Trim() == "Attainment");
        Assert.Equal(attainment, term.NextElementSibling!.TextContent.Trim());
    }

    private string SummaryValue(string name, string label)
    {
        var section = RepSections.Single(candidate => candidate.QuerySelector("h2")!.TextContent.Trim() == name);
        return section.QuerySelectorAll("dl dt").Single(term => term.TextContent.Trim() == label).NextElementSibling!.TextContent.Trim();
    }

    [Then("every rep table lists exactly the engine's lines for {string}")]
    public void ThenEveryRepTableListsTheEnginesLines(string fileName)
    {
        var statements = EngineStatements(fileName);
        Assert.Equal(statements.Count, RepSections.Count);
        foreach (var (statement, section) in statements.Zip(RepSections))
        {
            var rows = section.QuerySelectorAll("tbody tr")
                .Select(row => row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToList())
                .ToList();
            Assert.Equal(statement.Lines.Select(line => new List<string> { line.Description, Format.Money(line.Amount), line.RequirementId }), rows);
        }
    }

    [Then("the page reports that the scenario was rejected with {int} errors")]
    public void ThenTheScenarioIsRejectedWith(int count)
    {
        var alert = Assert.Single(Document.QuerySelectorAll("[role='alert']"));
        Assert.Equal(count, alert.QuerySelectorAll("li").Length);
    }

    [Then("the rejection cites the requirements {string}")]
    public void ThenTheRejectionCitesTheRequirements(string requirements)
    {
        var alert = Assert.Single(Document.QuerySelectorAll("[role='alert']"));
        var cited = alert.QuerySelectorAll("li").Select(item => item.QuerySelector(".requirement")!.TextContent.Trim()).Order(StringComparer.Ordinal);

        Assert.Equal(List(requirements).Order(StringComparer.Ordinal), cited);
    }

    [Then("the page shows the reps {string}")]
    public void ThenThePageShowsTheReps(string names) =>
        Assert.Equal(List(names), RepSections.Select(section => section.QuerySelector("h2")!.TextContent.Trim()));

    [Then("the page says {string}")]
    public void ThenThePageSays(string text) =>
        Assert.Contains(text, Document.QuerySelector("main")!.TextContent, StringComparison.Ordinal);

    [Then("the page reports that {string} failed to load")]
    public void ThenThePageReportsALoadFailure(string fileName) =>
        Assert.Contains(Document.QuerySelectorAll(".load-errors li"), item => item.TextContent.Contains(fileName, StringComparison.Ordinal));

    private string StartWith(IReadOnlyList<string> fileNames)
    {
        var directory = host.CreateScenarioDirectory();
        foreach (var fileName in fileNames)
        {
            File.Copy(Fixtures.ScenarioPath(fileName), Path.Combine(directory, fileName));
        }

        _client = host.Start(directory);
        return directory;
    }

    private async Task Request(string path)
    {
        _response = await Client.GetAsync(new Uri(path, UriKind.Relative));
        _document = new HtmlParser().ParseDocument(await _response.Content.ReadAsStringAsync());
    }

    private static IReadOnlyList<RepStatement> EngineStatements(string fileName)
    {
        var read = Assert.IsType<ScenarioRead>(ScenarioFileReader.Read(fileName, File.ReadAllText(Fixtures.ScenarioPath(fileName))));
        return Assert.IsType<CalculatedScenario>(CommissionEngine.Calculate(read.File.Scenario)).Statements;
    }

    private static List<string> List(string commaSeparated) =>
        commaSeparated.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

    private IElement Picker => Assert.Single(Document.QuerySelectorAll("form"));

    private List<IElement> RepSections => Document.QuerySelectorAll("section.rep").ToList();

    private HttpClient Client => _client ?? throw new InvalidOperationException("The application was not started.");

    private HttpResponseMessage Response => _response ?? throw new InvalidOperationException("No page was requested.");

    private IHtmlDocument Document => _document ?? throw new InvalidOperationException("No page was requested.");

    [GeneratedRegex(@"\*\*(FR-\d{3})\*\*")]
    private static partial Regex SpecRequirement();
}
