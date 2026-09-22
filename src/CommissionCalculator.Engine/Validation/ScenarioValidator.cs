using System.Globalization;
using CommissionCalculator.Engine.Calculation;

namespace CommissionCalculator.Engine.Validation;

// FR-004's shared rules. Each applies wherever its input occurs: the scenario's roster and deal
// list, and every booking quarter's reps and deals.
[Implements("FR-004")]
internal static class ScenarioValidator
{
    private const string Requirement = "FR-004";

    public static IReadOnlyList<ValidationError> Validate(ScenarioInput scenario) =>
        ScenarioErrors(scenario)
            .Concat(scenario.BookingQuarters.SelectMany(BookingQuarterErrors))
            .Select(message => new ValidationError(message, Requirement))
            .ToList();

    private static IEnumerable<string> ScenarioErrors(ScenarioInput scenario)
    {
        if (scenario.Roster.Count == 0)
        {
            yield return $"Scenario '{scenario.Id}': roster is empty.";
        }

        foreach (var error in QuarterErrors(scenario.Quarter)
                     .Concat(scenario.Roster.SelectMany(RepErrors))
                     .Concat(DuplicateRepIds(scenario.Roster.Select(rep => rep.RepId)))
                     .Concat(DealListErrors(scenario.Deals))
                     .Concat(UnknownReps(scenario)))
        {
            yield return error;
        }
    }

    private static IEnumerable<string> BookingQuarterErrors(BookingQuarterInput bookingQuarter) =>
        QuarterErrors(bookingQuarter.Quarter)
            .Concat(bookingQuarter.Reps.SelectMany(rep => QuotaErrors(rep.RepId, rep.Quota)))
            .Concat(DealListErrors(bookingQuarter.Deals));

    private static IEnumerable<string> QuarterErrors(QuarterPeriod quarter)
    {
        if (!Quarters.IsWellFormed(quarter))
        {
            yield return $"Quarter {Date(quarter.Start)}..{Date(quarter.End)} is not three whole calendar months starting on the 1st.";
        }
    }

    private static IEnumerable<string> RepErrors(RepInput rep)
    {
        foreach (var error in QuotaErrors(rep.RepId, rep.Quota))
        {
            yield return error;
        }

        if (rep.OpeningRecoverableBalance < 0m)
        {
            yield return $"Rep '{rep.RepId}': opening recoverable balance must not be negative.";
        }

        if (!Money.IsWholeCents(rep.OpeningRecoverableBalance))
        {
            yield return $"Rep '{rep.RepId}': opening recoverable balance {Amount(rep.OpeningRecoverableBalance)} is not a whole number of cents.";
        }
    }

    private static IEnumerable<string> QuotaErrors(string repId, decimal quota)
    {
        if (quota <= 0m)
        {
            yield return $"Rep '{repId}': quota must be greater than zero.";
        }

        if (!Money.IsWholeCents(quota))
        {
            yield return $"Rep '{repId}': quota {Amount(quota)} is not a whole number of cents.";
        }
    }

    private static IEnumerable<string> DealListErrors(IReadOnlyList<DealInput> deals) =>
        deals.SelectMany(DealErrors)
            .Concat(deals.GroupBy(deal => deal.DealId)
                .Where(group => group.Count() > 1)
                .Select(group => $"Deal '{group.Key}' appears more than once in the deal list."));

    private static IEnumerable<string> DealErrors(DealInput deal)
    {
        if (deal.Amount <= 0m)
        {
            yield return $"Deal '{deal.DealId}': amount must be greater than zero.";
        }

        if (!Money.IsWholeCents(deal.Amount))
        {
            yield return $"Deal '{deal.DealId}': amount {Amount(deal.Amount)} is not a whole number of cents.";
        }

        foreach (var error in deal.Refunds.SelectMany(refund => RefundErrors(deal, refund))
                     .Concat(RefundTotalErrors(deal))
                     .Concat(SplitErrors(deal)))
        {
            yield return error;
        }
    }

    private static IEnumerable<string> RefundTotalErrors(DealInput deal)
    {
        if (deal.Refunds.Sum(refund => refund.Amount) > deal.Amount)
        {
            yield return $"Deal '{deal.DealId}': refunds total more than the deal amount.";
        }
    }

    private static IEnumerable<string> RefundErrors(DealInput deal, RefundInput refund)
    {
        var dealId = deal.DealId;
        if (refund.Date < deal.BookingDate)
        {
            yield return $"Deal '{dealId}': a refund dated {Date(refund.Date)} is dated before its booking date {Date(deal.BookingDate)}.";
        }

        if (refund.Amount <= 0m)
        {
            yield return $"Deal '{dealId}': refund amount must be greater than zero.";
        }

        if (!Money.IsWholeCents(refund.Amount))
        {
            yield return $"Deal '{dealId}': refund amount {Amount(refund.Amount)} is not a whole number of cents.";
        }
    }

    private static IEnumerable<string> SplitErrors(DealInput deal) =>
        deal.Splits
            .Where(split => split.Percent <= 0m || split.Percent > 100m)
            .Select(split => $"Deal '{deal.DealId}': rep '{split.RepId}' has split percentage {Amount(split.Percent)}%; a split percentage must be greater than 0% and at most 100%.")
            .Concat(deal.Splits.GroupBy(split => split.RepId)
                .Where(group => group.Count() > 1)
                .Select(group => $"Rep '{group.Key}' appears more than once on deal '{deal.DealId}'."));

    private static IEnumerable<string> DuplicateRepIds(IEnumerable<string> repIds) =>
        repIds.GroupBy(repId => repId)
            .Where(group => group.Count() > 1)
            .Select(group => $"Rep '{group.Key}' appears more than once on the roster.");

    private static IEnumerable<string> UnknownReps(ScenarioInput scenario)
    {
        var roster = scenario.Roster.Select(rep => rep.RepId).ToHashSet();
        return scenario.Deals
            .SelectMany(deal => deal.Splits
                .Where(split => !roster.Contains(split.RepId))
                .Select(split => $"Deal '{deal.DealId}' is credited to rep '{split.RepId}', who is not on the roster."));
    }

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Amount(decimal amount) => amount.ToString(CultureInfo.InvariantCulture);
}
