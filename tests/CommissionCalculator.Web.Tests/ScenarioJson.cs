namespace CommissionCalculator.Web.Tests;

internal static class ScenarioJson
{
    // The example in contracts/scenario-file.md (Appendix A.1, Avery only).
    public const string Tiers = """
        {
          "id": "tiers",
          "name": "Tiered rates",
          "description": "One rep per tier example in spec US1 (excerpt of Appendix A.1: Avery only).",
          "quarter": { "start": "2026-01-01", "end": "2026-03-31" },
          "roster": [
            { "repId": "avery", "name": "Avery", "quota": 100000.00,
              "startDate": "2025-06-01", "openingRecoverableBalance": 0.00 }
          ],
          "deals": [
            { "dealId": "T-1", "amount": 80000.00, "closeDate": "2026-02-10",
              "bookingDate": "2026-02-12",
              "splits": [ { "repId": "avery", "percent": 100 } ],
              "refunds": [] }
          ],
          "bookingQuarters": []
        }
        """;

    public static string WithId(string id) => Tiers.Replace("\"id\": \"tiers\"", $"\"id\": \"{id}\"", StringComparison.Ordinal);
}
