using Xunit;

namespace CommissionCalculator.Tools.Tests;

[Trait("Principle", "II")]
public sealed class TrxTestListTests
{
    [Fact]
    public void ListsClassAndMethodNamesIncludingReqnrollScenarios()
    {
        var trx = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "two-tests.trx"));

        var tests = TrxTestList.List(trx);

        Assert.Equal(
            ["CommissionCalculator.Engine.Tests.MoneyTests.RoundsHalfAwayFromZero",
             "CommissionCalculator.Specs.Features.AppHostFeature.TheAppServesItsPage"],
            tests);
    }
}
