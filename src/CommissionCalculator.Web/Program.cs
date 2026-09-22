using CommissionCalculator.Web.Catalog;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.Configure<ScenarioOptions>(builder.Configuration.GetSection("Scenarios"));
builder.Services.AddSingleton(services =>
    new ScenarioCatalog(services.GetRequiredService<IOptions<ScenarioOptions>>().Value.Directory));

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();

// The composition root: Razor Pages, options bound from configuration and a catalogue reading
// local files — no external service, no authentication and no database (FR-020).
[CommissionCalculator.Engine.Implements("FR-020")]
public partial class Program;
