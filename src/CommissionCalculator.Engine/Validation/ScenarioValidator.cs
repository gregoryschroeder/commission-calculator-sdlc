using System.Globalization;
using CommissionCalculator.Engine.Calculation;

namespace CommissionCalculator.Engine.Validation;

// FR-004's shared rules. Each applies wherever its input occurs: the scenario's roster and deal
// list, and every booking quarter's reps and deals.
[Implements("FR-004")]
internal static class ScenarioValidator
{
    private const string Requirement4 = "FR-004";

    public static IReadOnlyList<ValidationError> Validate(ScenarioInput scenario) =>
        ScenarioErrors(scenario)
            .Concat(scenario.BookingQuarters.SelectMany(BookingQuarterErrors))
            .Concat(BookingQuarterDataErrors(scenario))
            .Select(error => new ValidationError(error.Message, error.Requirement))
            .ToList();

    // Each message carries the requirement it enforces, as Appendix A.11 states them: the
    // start-date rules are FR-011's, the split-sum rule is FR-013's, the rest are FR-004's.
    private sealed record Failure(string Message, string Requirement = Requirement4);

    [Implements("FR-011")]
    private static Failure StartDateFailure(string message) => new(message, "FR-011");

    [Implements("FR-013")]
    private static Failure SplitSumFailure(string message) => new(message, "FR-013");

    private static IEnumerable<Failure> StartDateErrors(string repId, DateOnly startDate, decimal quota, QuarterPeriod quarter)
    {
        if (startDate > quarter.End)
        {
            yield return StartDateFailure($"Rep '{repId}' starts after the quarter ends ({Date(quarter.End)}).");
            yield break;
        }

        if (QuotaProration.For(quota, startDate, quarter).Amount == 0m)
        {
            yield return new Failure($"Rep '{repId}': the prorated quota rounds to $0.00.");
        }
    }

    private static IEnumerable<Failure> BookedBeforeStartErrors(IReadOnlyList<DealInput> deals, Dictionary<string, DateOnly> startDates) =>
        deals.SelectMany(deal => deal.Splits
            .Where(split => startDates.TryGetValue(split.RepId, out var start) && deal.BookingDate < start)
            .Select(split => StartDateFailure($"Deal '{deal.DealId}' is booked before the start date of rep '{split.RepId}' ({Date(startDates[split.RepId])})."))); 

    // The booking-quarter data a clawback needs, as far as the engine can detect it missing
    // (clarification 2026-09-22): the quarter itself, an entry for each credited roster rep, and a
    // start date for each split partner. That the data lists *all* of those deals is a
    // precondition on the scenario, not a check.
    private static IEnumerable<Failure> BookingQuarterDataErrors(ScenarioInput scenario)
    {
        var roster = scenario.Roster.Select(rep => rep.RepId).ToHashSet();
        var startDates = scenario.Roster.ToLookup(rep => rep.RepId, rep => rep.StartDate);

        foreach (var bookingQuarter in scenario.BookingQuarters)
        {
            if (bookingQuarter.Quarter.Start <= scenario.Quarter.End && scenario.Quarter.Start <= bookingQuarter.Quarter.End)
            {
                yield return new Failure($"Booking quarter {Date(bookingQuarter.Quarter.Start)}..{Date(bookingQuarter.Quarter.End)} overlaps the scenario's quarter.");
            }

            foreach (var deal in bookingQuarter.Deals.Where(deal => deal.BookingDate < bookingQuarter.Quarter.Start || deal.BookingDate > bookingQuarter.Quarter.End))
            {
                yield return new Failure($"Deal '{deal.DealId}' is booked outside its booking quarter {Date(bookingQuarter.Quarter.Start)}..{Date(bookingQuarter.Quarter.End)}.");
            }

            foreach (var rep in bookingQuarter.Reps.Where(rep => startDates[rep.RepId].Any(start => start != rep.StartDate)))
            {
                yield return new Failure($"Rep '{rep.RepId}': the booking quarter's start date differs from the roster ({Date(rep.StartDate)}).");
            }

            var known = bookingQuarter.Reps.Select(rep => rep.RepId)
                .Concat(bookingQuarter.Partners.Select(partner => partner.RepId)).ToHashSet();
            foreach (var deal in bookingQuarter.Deals)
            {
                foreach (var split in deal.Splits.Where(split => !known.Contains(split.RepId)))
                {
                    yield return roster.Contains(split.RepId)
                        ? new Failure($"Rep '{split.RepId}' is credited on booking-quarter deal '{deal.DealId}' but has no entry in the booking quarter's reps.")
                        : new Failure($"Rep '{split.RepId}' on booking-quarter deal '{deal.DealId}' is neither on the roster nor listed as a partner with a start date.");
                }
            }
        }

        foreach (var error in DuplicatedDealErrors(scenario).Concat(MissingBookingQuarterErrors(scenario)))
        {
            yield return error;
        }
    }

    private static IEnumerable<Failure> DuplicatedDealErrors(ScenarioInput scenario)
    {
        var bookingDeals = scenario.BookingQuarters.SelectMany(quarter => quarter.Deals).ToLookup(deal => deal.DealId);
        return scenario.Deals
            .Where(deal => bookingDeals[deal.DealId].Any(other => other != deal))
            .Select(deal => new Failure($"Deal '{deal.DealId}' differs between the two lists."));
    }

    private static IEnumerable<Failure> MissingBookingQuarterErrors(ScenarioInput scenario) =>
        scenario.Deals
            .Where(deal => deal.BookingDate < scenario.Quarter.Start
                && deal.Refunds.Any(refund => refund.Date >= scenario.Quarter.Start && refund.Date <= scenario.Quarter.End)
                && !scenario.BookingQuarters.Any(quarter => deal.BookingDate >= quarter.Quarter.Start && deal.BookingDate <= quarter.Quarter.End))
            .Select(deal => new Failure($"Deal '{deal.DealId}', refunded in this quarter, was booked {Date(deal.BookingDate)} and has no booking-quarter data."));

    private static IEnumerable<Failure> ScenarioErrors(ScenarioInput scenario)
    {
        if (scenario.Roster.Count == 0)
        {
            yield return new Failure($"Scenario '{scenario.Id}': roster is empty.");
        }

        foreach (var error in QuarterErrors(scenario.Quarter)
                     .Concat(scenario.Roster.SelectMany(RepErrors))
                     .Concat(DuplicateRepIds(scenario.Roster.Select(rep => rep.RepId)))
                     .Concat(DealListErrors(scenario.Deals))
                     .Concat(scenario.Roster.SelectMany(rep => StartDateErrors(rep.RepId, rep.StartDate, rep.Quota, scenario.Quarter)))
                     .Concat(BookedBeforeStartErrors(scenario.Deals, EarliestStartDates(scenario.Roster.Select(rep => (rep.RepId, rep.StartDate)))))
                     .Concat(UnknownReps(scenario)))
        {
            yield return error;
        }
    }

    private static IEnumerable<Failure> BookingQuarterErrors(BookingQuarterInput bookingQuarter) =>
        QuarterErrors(bookingQuarter.Quarter)
            .Concat(bookingQuarter.Reps.SelectMany(rep => QuotaErrors(rep.RepId, rep.Quota)))
            .Concat(bookingQuarter.Reps.SelectMany(rep => StartDateErrors(rep.RepId, rep.StartDate, rep.Quota, bookingQuarter.Quarter)))
            .Concat(BookedBeforeStartErrors(bookingQuarter.Deals, StartDates(bookingQuarter)))
            .Concat(DealListErrors(bookingQuarter.Deals));

    // A split partner with no start date is skipped here; rejecting that is FR-004's own rule,
    // added with the booking-quarter checks in Phase 8 (tasks T044, T056).
    private static Dictionary<string, DateOnly> StartDates(BookingQuarterInput bookingQuarter) =>
        EarliestStartDates(bookingQuarter.Reps.Select(rep => (rep.RepId, rep.StartDate))
            .Concat(bookingQuarter.Partners.Select(partner => (partner.RepId, partner.StartDate))));

    // Duplicate ids are themselves a rejection (FR-004), and the engine never throws on invalid
    // data, so a repeated id keeps its earliest start date here rather than failing the lookup.
    private static Dictionary<string, DateOnly> EarliestStartDates(IEnumerable<(string RepId, DateOnly StartDate)> entries) =>
        entries.GroupBy(entry => entry.RepId).ToDictionary(group => group.Key, group => group.Min(entry => entry.StartDate));

    private static IEnumerable<Failure> QuarterErrors(QuarterPeriod quarter)
    {
        if (!Quarters.IsWellFormed(quarter))
        {
            yield return new Failure($"Quarter {Date(quarter.Start)}..{Date(quarter.End)} is not three whole calendar months starting on the 1st.");
        }
    }

    private static IEnumerable<Failure> RepErrors(RepInput rep)
    {
        foreach (var error in QuotaErrors(rep.RepId, rep.Quota))
        {
            yield return error;
        }

        if (rep.OpeningRecoverableBalance < 0m)
        {
            yield return new Failure($"Rep '{rep.RepId}': opening recoverable balance must not be negative.");
        }

        if (!Money.IsWholeCents(rep.OpeningRecoverableBalance))
        {
            yield return new Failure($"Rep '{rep.RepId}': opening recoverable balance {Amount(rep.OpeningRecoverableBalance)} is not a whole number of cents.");
        }
    }

    private static IEnumerable<Failure> QuotaErrors(string repId, decimal quota)
    {
        if (quota <= 0m)
        {
            yield return new Failure($"Rep '{repId}': quota must be greater than zero.");
        }

        if (!Money.IsWholeCents(quota))
        {
            yield return new Failure($"Rep '{repId}': quota {Amount(quota)} is not a whole number of cents.");
        }
    }

    private static IEnumerable<Failure> DealListErrors(IReadOnlyList<DealInput> deals) =>
        deals.SelectMany(DealErrors)
            .Concat(deals.GroupBy(deal => deal.DealId)
                .Where(group => group.Count() > 1)
                .Select(group => new Failure($"Deal '{group.Key}' appears more than once in the deal list.")));

    private static IEnumerable<Failure> DealErrors(DealInput deal)
    {
        if (deal.Amount <= 0m)
        {
            yield return new Failure($"Deal '{deal.DealId}': amount must be greater than zero.");
        }

        if (!Money.IsWholeCents(deal.Amount))
        {
            yield return new Failure($"Deal '{deal.DealId}': amount {Amount(deal.Amount)} is not a whole number of cents.");
        }

        foreach (var error in deal.Refunds.SelectMany(refund => RefundErrors(deal, refund))
                     .Concat(RefundTotalErrors(deal))
                     .Concat(SplitSumErrors(deal))
                     .Concat(SplitErrors(deal)))
        {
            yield return error;
        }
    }

    private static IEnumerable<Failure> SplitSumErrors(DealInput deal)
    {
        var sum = deal.Splits.Sum(split => split.Percent);
        if (sum != 100m)
        {
            yield return SplitSumFailure($"Deal '{deal.DealId}': split percentages sum to {sum.ToString("0.###", CultureInfo.InvariantCulture)}%, not 100%.");
        }
    }

    private static IEnumerable<Failure> RefundTotalErrors(DealInput deal)
    {
        if (deal.Refunds.Sum(refund => refund.Amount) > deal.Amount)
        {
            yield return new Failure($"Deal '{deal.DealId}': refunds total more than the deal amount.");
        }
    }

    private static IEnumerable<Failure> RefundErrors(DealInput deal, RefundInput refund)
    {
        var dealId = deal.DealId;
        if (refund.Date < deal.BookingDate)
        {
            yield return new Failure($"Deal '{dealId}': a refund dated {Date(refund.Date)} is dated before its booking date {Date(deal.BookingDate)}.");
        }

        if (refund.Amount <= 0m)
        {
            yield return new Failure($"Deal '{dealId}': refund amount must be greater than zero.");
        }

        if (!Money.IsWholeCents(refund.Amount))
        {
            yield return new Failure($"Deal '{dealId}': refund amount {Amount(refund.Amount)} is not a whole number of cents.");
        }
    }

    private static IEnumerable<Failure> SplitErrors(DealInput deal) =>
        deal.Splits
            .Where(split => split.Percent <= 0m || split.Percent > 100m)
            .Select(split => new Failure($"Deal '{deal.DealId}': rep '{split.RepId}' has split percentage {Amount(split.Percent)}%; a split percentage must be greater than 0% and at most 100%."))
            .Concat(deal.Splits.GroupBy(split => split.RepId)
                .Where(group => group.Count() > 1)
                .Select(group => new Failure($"Rep '{group.Key}' appears more than once on deal '{deal.DealId}'.")));

    private static IEnumerable<Failure> DuplicateRepIds(IEnumerable<string> repIds) =>
        repIds.GroupBy(repId => repId)
            .Where(group => group.Count() > 1)
            .Select(group => new Failure($"Rep '{group.Key}' appears more than once on the roster."));

    private static IEnumerable<Failure> UnknownReps(ScenarioInput scenario)
    {
        var roster = scenario.Roster.Select(rep => rep.RepId).ToHashSet();
        return scenario.Deals
            .SelectMany(deal => deal.Splits
                .Where(split => !roster.Contains(split.RepId))
                .Select(split => new Failure($"Deal '{deal.DealId}' is credited to rep '{split.RepId}', who is not on the roster.")));
    }

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Amount(decimal amount) => amount.ToString(CultureInfo.InvariantCulture);
}
