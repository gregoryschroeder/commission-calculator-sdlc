namespace CommissionCalculator.Engine
{
    // Declared here, and not referenced from the engine, so the trace is proved to match the
    // attribute by its full type name rather than by identity (task T061).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class ImplementsAttribute(string requirementId) : Attribute
    {
        public string RequirementId { get; } = requirementId;
    }
}

namespace CommissionCalculator.Tools.Tests
{
    using CommissionCalculator.Engine;

    [Implements("FR-001")]
    internal sealed class TracedFixture
    {
        [Implements("FR-002")]
        public static void TracedMethod()
        {
        }
    }
}
