using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace CommissionCalculator.Specs.Support;

[Binding]
public sealed class AppHost : IDisposable
{
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Start()
    {
        _factory = new WebApplicationFactory<Program>();
        return _factory.CreateClient();
    }

    public void Dispose() => _factory?.Dispose();
}
