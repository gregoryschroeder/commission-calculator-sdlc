using CommissionCalculator.Engine.Calculation;
using CommissionCalculator.Engine.Validation;

namespace CommissionCalculator.Engine;

public static class CommissionEngine
{
    // Validates, then calculates. Never throws for invalid scenario data: invalid data is a
    // Rejected result (FR-004). Deterministic: same input, same output.
    public static ScenarioResult Calculate(ScenarioInput scenario)
    {
        var errors = ScenarioValidator.Validate(scenario);
        if (errors.Count > 0)
        {
            return new RejectedScenario(errors);
        }

        return new CalculatedScenario(scenario.Roster.Select(StatementBuilder.Build).ToList());
    }
}
