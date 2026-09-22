using System.Globalization;

namespace CommissionCalculator.Web.Presentation;

public static class Format
{
    private const string MinusSign = "−";

    // A negative amount carries a minus sign in its text, never colour alone (FR-021).
    [Engine.Implements("FR-021")]
    public static string Money(decimal amount) =>
        (amount < 0m ? MinusSign : "") + "$" + Math.Abs(amount).ToString("#,##0.00", CultureInfo.InvariantCulture);

    // Two decimals, half away from zero; display only (FR-002).
    [Engine.Implements("FR-002")]
    public static string Attainment(decimal ratio) =>
        Math.Round(ratio * 100m, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture) + "%";
}
