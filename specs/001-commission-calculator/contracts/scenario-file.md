# Contract: Seeded scenario file

One UTF-8 JSON file per scenario in `src/CommissionCalculator.Web/Scenarios/*.json`, read into a
web-side `ScenarioFile` record. `description` stays in the web catalog for display in the picker
and page heading; every other property maps 1:1 to `ScenarioInput` (engine-api.md), which has no
description because the engine does not use one. Property names are camelCase; dates are `YYYY-MM-DD`; money and
percentages are JSON numbers read as `decimal` (exact — research R10). Unknown properties are an
error. A file that is not valid JSON or does not match this shape is reported on the page as a
scenario that failed to load, with the parser's message; it never crashes the app.

```json
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
```

`bookingQuarters[]` items: `{ "quarter": {...}, "reps": [{repId, quota, startDate}],
"partners": [{repId, startDate}], "deals": [ ...Deal ] }`.

`openingRecoverableBalance`, `refunds` and `bookingQuarters` may be omitted (default 0.00 / empty).
