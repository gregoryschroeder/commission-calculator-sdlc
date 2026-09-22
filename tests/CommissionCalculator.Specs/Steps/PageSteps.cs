using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CommissionCalculator.Specs.Support;
using Reqnroll;
using Xunit;

namespace CommissionCalculator.Specs.Steps;

[Binding]
public sealed class PageSteps(AppHost host)
{
    private HttpClient? _client;
    private HttpResponseMessage? _response;
    private IHtmlDocument? _document;

    [Given("the application is running")]
    public void GivenTheApplicationIsRunning() => _client = host.Start();

    [When("the home page is requested")]
    public async Task WhenTheHomePageIsRequested()
    {
        _response = await Client.GetAsync(new Uri("/", UriKind.Relative));
        _document = new HtmlParser().ParseDocument(await _response.Content.ReadAsStringAsync());
    }

    [Then("the response status is {int}")]
    public void ThenTheResponseStatusIs(int status) => Assert.Equal((HttpStatusCode)status, Response.StatusCode);

    [Then("the page language is {string}")]
    public void ThenThePageLanguageIs(string language) =>
        Assert.Equal(language, Document.DocumentElement.GetAttribute("lang"));

    [Then("the page has exactly one main landmark")]
    public void ThenThePageHasExactlyOneMainLandmark() => Assert.Single(Document.QuerySelectorAll("main"));

    private HttpClient Client => _client ?? throw new InvalidOperationException("The application was not started.");
    private HttpResponseMessage Response => _response ?? throw new InvalidOperationException("No page was requested.");
    private IHtmlDocument Document => _document ?? throw new InvalidOperationException("No page was requested.");
}
