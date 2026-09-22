namespace CommissionCalculator.Engine.Calculation;

internal sealed record ClawbackEntry(DealInput Deal, RefundInput Refund, decimal Amount, decimal? ResplitShare, decimal ReducedAmount);

// When a deal is refunded, the commission on it is clawed back in the statement of the quarter
// holding the refund date, sized by recomputing the booking quarter with and without that refund
// (clarifications 2026-09-21, FR-016/FR-017). Refunds on one booking quarter's deals are applied
// in refund-date order across deals, so the clawbacks total exactly what the refunded revenue
// earned.
[Implements("FR-016")]
[Implements("FR-017")]
internal static class ClawbackCalculator
{
    public static IReadOnlyList<ClawbackEntry> For(RepInput rep, ScenarioInput scenario) =>
        Contexts(rep, scenario).SelectMany(context => Entries(context, scenario.Quarter)).ToList();

    private static IEnumerable<QuarterContext> Contexts(RepInput rep, ScenarioInput scenario)
    {
        yield return new QuarterContext(rep.RepId, rep.Quota, rep.StartDate, scenario.Quarter, scenario.Deals);

        foreach (var bookingQuarter in scenario.BookingQuarters)
        {
            var entry = bookingQuarter.Reps.FirstOrDefault(candidate => candidate.RepId == rep.RepId);
            if (entry is not null)
            {
                yield return new QuarterContext(rep.RepId, entry.Quota, entry.StartDate, bookingQuarter.Quarter, bookingQuarter.Deals);
            }
        }
    }

    private static IEnumerable<ClawbackEntry> Entries(QuarterContext context, QuarterPeriod statementQuarter)
    {
        var applied = new HashSet<RefundKey>();
        foreach (var refund in OrderedRefunds(context))
        {
            var before = QuarterCommission.For(context, applied);
            applied.Add(new RefundKey(refund.Deal.DealId, refund.Index));
            var after = QuarterCommission.For(context, applied);

            if (refund.Refund.Date < statementQuarter.Start || refund.Refund.Date > statementQuarter.End)
            {
                continue;
            }

            var reduced = QuarterCredit.ReducedAmount(refund.Deal, applied);
            var splitIndex = refund.Deal.Splits.ToList().FindIndex(split => split.RepId == context.RepId);
            decimal? resplitShare = refund.Deal.Splits.Count > 1
                ? SplitAllocation.Allocate(reduced, refund.Deal.Splits)[splitIndex]
                : null;

            yield return new ClawbackEntry(refund.Deal, refund.Refund, before - after, resplitShare, reduced);
        }
    }

    private static IEnumerable<(DealInput Deal, int Index, RefundInput Refund, int DealPosition)> OrderedRefunds(QuarterContext context) =>
        context.Deals
            .Select((deal, position) => (deal, position))
            .Where(entry => entry.deal.Splits.Any(split => split.RepId == context.RepId)
                && entry.deal.BookingDate >= context.Quarter.Start && entry.deal.BookingDate <= context.Quarter.End)
            .SelectMany(entry => entry.deal.Refunds.Select((refund, index) => (Deal: entry.deal, Index: index, Refund: refund, DealPosition: entry.position)))
            .OrderBy(entry => entry.Refund.Date)
            .ThenBy(entry => entry.DealPosition)
            .ThenBy(entry => entry.Index);
}
