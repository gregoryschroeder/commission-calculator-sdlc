using System.Globalization;
using System.Reflection;
using CommissionCalculator.Tools;

switch (args)
{
    case ["coverage-gate", var floor, var engineAssembly, .. var reports] when reports.Length > 0:
        var result = CoverageGate.Evaluate(
            reports.Select(File.ReadAllText).ToList(),
            engineHasTypes: Assembly.LoadFrom(engineAssembly).GetTypes().Length > 0,
            floorPercent: decimal.Parse(floor, CultureInfo.InvariantCulture));
        Console.WriteLine(result.Message);
        return result.Passed ? 0 : 1;

    case ["list-tests", var trx]:
        foreach (var test in TrxTestList.List(File.ReadAllText(trx)))
        {
            Console.WriteLine(test);
        }

        return 0;

    default:
        Console.Error.WriteLine("usage: coverage-gate <floor-percent> <engine.dll> <cobertura.xml>...");
        Console.Error.WriteLine("       list-tests <results.trx>");
        return 2;
}
