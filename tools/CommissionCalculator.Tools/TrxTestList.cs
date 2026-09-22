using System.Xml.Linq;

namespace CommissionCalculator.Tools;

internal static class TrxTestList
{
    // Reqnroll scenarios are listed by display name, so the runnable name comes from TestMethod.
    public static IReadOnlyList<string> List(string trxXml) =>
        XDocument.Parse(trxXml).Descendants()
            .Where(element => element.Name.LocalName == "TestMethod")
            .Select(method => $"{(string?)method.Attribute("className")}.{(string?)method.Attribute("name")}")
            .ToList();
}
