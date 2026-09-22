using CommissionCalculator.Engine;
using CommissionCalculator.Web.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommissionCalculator.Web.Pages;

// A view of the engine: it picks a scenario and renders the engine's statements without
// computing any amount itself (constitution Principle V).
[Implements("FR-001")]
[Implements("FR-002")]
[Implements("FR-003")]
public sealed class IndexModel(ScenarioCatalog catalog) : PageModel
{
    public IReadOnlyList<ScenarioFile> Scenarios => catalog.Scenarios;

    public IReadOnlyList<ScenarioLoadError> LoadErrors => catalog.LoadErrors;

    public ScenarioFile? Selected { get; private set; }

    public string? NotFoundId { get; private set; }

    public ScenarioResult? Result { get; private set; }

    public IActionResult OnGet([FromQuery] string? scenario)
    {
        Selected = scenario is null
            ? (catalog.Scenarios.Count > 0 ? catalog.Scenarios[0] : null)
            : catalog.Scenarios.FirstOrDefault(file => file.Scenario.Id == scenario);

        if (scenario is not null && Selected is null)
        {
            NotFoundId = scenario;
            Response.StatusCode = StatusCodes.Status404NotFound;
        }

        Result = Selected is null ? null : CommissionEngine.Calculate(Selected.Scenario);
        return Page();
    }
}
