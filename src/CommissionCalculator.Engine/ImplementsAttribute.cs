namespace CommissionCalculator.Engine;

// Marks the member that implements a requirement; read by the tools project's `trace` command.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ImplementsAttribute(string requirementId) : Attribute
{
    public string RequirementId { get; } = requirementId;
}
