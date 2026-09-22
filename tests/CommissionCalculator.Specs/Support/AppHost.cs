using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace CommissionCalculator.Specs.Support;

// Starts the web app in memory. A scenario that needs scenarios gives it a folder it created
// itself, so no test depends on shared files or on another test (constitution Principle II).
[Binding]
public sealed class AppHost : IDisposable
{
    private WebApplicationFactory<Program>? _factory;
    private string? _scenarioDirectory;

    public HttpClient Start() => Start(null);

    public HttpClient Start(string? scenarioDirectory)
    {
        _factory = new WebApplicationFactory<Program>();
        if (scenarioDirectory is not null)
        {
            _factory = _factory.WithWebHostBuilder(builder => builder.UseSetting("Scenarios:Directory", scenarioDirectory));
        }

        return _factory.CreateClient();
    }

    public string CreateScenarioDirectory()
    {
        _scenarioDirectory = Path.Combine(Path.GetTempPath(), "specs-scenarios-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_scenarioDirectory);
        return _scenarioDirectory;
    }

    public void Dispose()
    {
        _factory?.Dispose();
        if (_scenarioDirectory is not null && Directory.Exists(_scenarioDirectory))
        {
            Directory.Delete(_scenarioDirectory, recursive: true);
        }
    }
}
