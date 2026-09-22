using System.Text.Json;
using System.Text.Json.Serialization;
using CommissionCalculator.Engine;

namespace CommissionCalculator.Web.Catalog;

public sealed record ScenarioFile(string FileName, string Description, ScenarioInput Scenario);

public sealed record ScenarioLoadError(string FileName, string Message);

public abstract record ScenarioReadResult;

public sealed record ScenarioRead(ScenarioFile File) : ScenarioReadResult;

public sealed record ScenarioReadFailed(ScenarioLoadError Error) : ScenarioReadResult;

// Reads one seeded scenario file (contracts/scenario-file.md). Anything that does not match the
// contract exactly is a load error naming the file, never an exception: this is the boundary.
[Engine.Implements("FR-001")]
public static class ScenarioFileReader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public static ScenarioReadResult Read(string fileName, string json)
    {
        try
        {
            var file = JsonSerializer.Deserialize<FileDto>(json, Options)
                ?? throw new JsonException("The file is empty.");
            return new ScenarioRead(new ScenarioFile(fileName, file.Description, file.ToInput()));
        }
        catch (JsonException exception)
        {
            return new ScenarioReadFailed(new ScenarioLoadError(fileName, exception.Message));
        }
    }

    private sealed record FileDto(string Id, string Name, string Description, QuarterDto Quarter,
        IReadOnlyList<RepDto> Roster, IReadOnlyList<DealDto> Deals, IReadOnlyList<BookingQuarterDto>? BookingQuarters = null)
    {
        public ScenarioInput ToInput() => new(Id, Name, Quarter.ToInput(),
            Roster.Select(rep => rep.ToInput()).ToList(),
            Deals.Select(deal => deal.ToInput()).ToList(),
            (BookingQuarters ?? []).Select(quarter => quarter.ToInput()).ToList());
    }

    private sealed record QuarterDto(DateOnly Start, DateOnly End)
    {
        public QuarterPeriod ToInput() => new(Start, End);
    }

    private sealed record RepDto(string RepId, string Name, decimal Quota, DateOnly StartDate, decimal OpeningRecoverableBalance = 0m)
    {
        public RepInput ToInput() => new(RepId, Name, Quota, StartDate, OpeningRecoverableBalance);
    }

    private sealed record SplitDto(string RepId, decimal Percent);

    private sealed record RefundDto(decimal Amount, DateOnly Date);

    private sealed record DealDto(string DealId, decimal Amount, DateOnly CloseDate, DateOnly BookingDate,
        IReadOnlyList<SplitDto> Splits, IReadOnlyList<RefundDto>? Refunds = null)
    {
        public DealInput ToInput() => new(DealId, Amount, CloseDate, BookingDate,
            Splits.Select(split => new SplitCredit(split.RepId, split.Percent)).ToList(),
            (Refunds ?? []).Select(refund => new RefundInput(refund.Amount, refund.Date)).ToList());
    }

    private sealed record BookingQuarterRepDto(string RepId, decimal Quota, DateOnly StartDate);

    private sealed record PartnerDto(string RepId, DateOnly StartDate);

    private sealed record BookingQuarterDto(QuarterDto Quarter, IReadOnlyList<BookingQuarterRepDto> Reps,
        IReadOnlyList<PartnerDto> Partners, IReadOnlyList<DealDto> Deals)
    {
        public BookingQuarterInput ToInput() => new(Quarter.ToInput(),
            Reps.Select(rep => new BookingQuarterRep(rep.RepId, rep.Quota, rep.StartDate)).ToList(),
            Partners.Select(partner => new Partner(partner.RepId, partner.StartDate)).ToList(),
            Deals.Select(deal => deal.ToInput()).ToList());
    }
}
