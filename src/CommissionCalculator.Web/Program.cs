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

public partial class Program;
