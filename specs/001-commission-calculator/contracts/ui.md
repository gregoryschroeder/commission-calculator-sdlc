# Contract: Web UI

Single Razor Page at `/`, server-rendered, no client script.

- `GET /` — the scenario picker and the first seeded scenario's results.
- `GET /?scenario={id}` — the picker with `{id}` selected and its results. An unknown id shows a
  "scenario not found" message (HTTP 404) and the picker.

## Structure (asserted by the web tests; FR-001–FR-004, FR-021, SC-002, SC-005)

- `<html lang="en">`, a `<title>` of the form "{scenario name} — Commission Calculator", one
  `<h1>`, a `<main>` landmark, and a skip link to it.
- Picker: `<form method="get">` containing `<label for="scenario">`, `<select id="scenario"
  name="scenario">` with one `<option>` per seeded scenario, and a `<button type="submit">`.
- Rejected scenario: an element with `role="alert"` listing each validation message and its FR.
- Each rep: a `<section aria-labelledby>` headed `<h2>` with the rep's name; a summary `<dl>`
  (quota, prorated quota, credited bookings, attainment as a percentage to two decimals, e.g.
  "80.00%", earned commission, draws paid, commission payable, closing recoverable balance); and
  a breakdown `<table>` with a `<caption>`, `<th scope="col">` headers **Item**, **Amount**,
  **Rule**, and one row per `BreakdownLine`, the Rule cell showing the line's FR ID.
- Amounts are formatted `$#,##0.00` with a leading minus sign for negatives (never colour alone).
- Focus is visible on every interactive element; colour contrast meets WCAG 2.2 AA.
