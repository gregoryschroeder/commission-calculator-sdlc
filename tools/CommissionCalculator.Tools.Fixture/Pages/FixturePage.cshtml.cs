using CommissionCalculator.Engine;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommissionCalculator.Tools.Fixture.Pages;

// A Razor Pages type carrying [Implements], so the traceability test proves the tool can reflect
// over an ASP.NET assembly (research R8).
[Implements("FR-001")]
public sealed class FixturePageModel : PageModel;
