using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public sealed class ResultService(IResultRepository results, IRegistrationRepository registrations, IRaceRepository races, ILeagueRepository leagues, IUnitOfWork unitOfWork) : IResultService
{
    public async Task ReplaceClassificationAsync(int raceId, IReadOnlyList<ClassificationEntryDTO> entries, CancellationToken cancellationToken = default)
    {
        var race = await races.GetAsync(raceId, cancellationToken) ?? throw new NotFoundException($"Race with id {raceId} not found.");
        var league = await leagues.GetAsync(race.LeagueId, cancellationToken) ?? throw new NotFoundException($"League with id {race.LeagueId} not found.");
        var errors = Validate(entries, race, league);
        var members = await registrations.GetByLeagueAsync(race.LeagueId, cancellationToken);
        var memberIds = members.Select(x => x.RegistrationId).ToHashSet();
        if (entries.Count > members.Count) errors.Add($"Classification cannot contain more than the league's {members.Count} registered members.");
        if (entries.Any(x => !memberIds.Contains(x.RegistrationId))) errors.Add("Every registration must belong to this race's league.");
        if (errors.Count > 0) throw new DomainValidationException(string.Join("; ", errors));
        var fastestLapEntry = entries
            .Where(x => !x.Dnf && x.FastestLapSeconds > 0)
            .OrderBy(x => x.FastestLapSeconds)
            .ThenBy(x => x.Position)
            .FirstOrDefault();
        var scored = entries.Select(e =>
        {
            var fastestLap = fastestLapEntry is not null && fastestLapEntry.RegistrationId == e.RegistrationId;
            var points = e.Dnf ? 0 : Score(league.PointsTable, e.Position) + (fastestLap ? league.FastestLapBonus : 0);
            var result = new Result(e.RegistrationId, raceId, e.Position, e.FastestLapSeconds, points,
                e.IncidentPoints, e.Dnf, e.Notes) { FastestLap = fastestLap };
            return result;
        }).ToList();
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await results.ReplaceRaceAsync(raceId, scored, ct);
            race.IsCompleted = scored.Count > 0;
            await races.UpdateAsync(race, ct);
        }, cancellationToken);
    }

    public async Task ClearClassificationAsync(int raceId, CancellationToken cancellationToken = default)
    {
        var race = await races.GetAsync(raceId, cancellationToken) ?? throw new NotFoundException($"Race with id {raceId} not found.");
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await results.ClearRaceAsync(raceId, ct); race.IsCompleted = false; await races.UpdateAsync(race, ct); }, cancellationToken);
    }

    private static List<string> Validate(IReadOnlyList<ClassificationEntryDTO> entries, Race race, League league)
    {
        var errors = new List<string>();
        if (race.ScheduledAt > DateTime.UtcNow) errors.Add("A future race cannot be classified.");
        if (entries.Count == 0) errors.Add("Classification must contain at least one driver.");
        if (entries.Count > league.MaxDrivers) errors.Add($"Classification cannot exceed {league.MaxDrivers} drivers.");
        if (!entries.Select(x => x.Position).Order().SequenceEqual(Enumerable.Range(1, entries.Count))) errors.Add("Positions must be exactly 1 through N with no gaps or duplicates.");
        if (entries.Select(x => x.RegistrationId).Distinct().Count() != entries.Count) errors.Add("Registration ids must be unique.");
        var finishers = entries.Where(x => !x.Dnf).ToList();
        if (entries.Any(x => x.Dnf && finishers.Any(f => f.Position > x.Position))) errors.Add("DNF positions must be after all finishers.");
        if (entries.Any(x => x.RegistrationId <= 0 || x.Position <= 0 || x.Position > 999 || x.IncidentPoints < 0 || x.IncidentPoints > 100 || x.FastestLapSeconds < 0 || x.FastestLapSeconds > 9999)) errors.Add("One or more classification values are out of range.");
        try
        {
            var points = league.PointsTable;
            if (points.Length is < 1 or > 50 || points.Any(x => x < 0 || x > 1000) || points.Zip(points.Skip(1)).Any(p => p.First < p.Second) || league.FastestLapBonus is < 0 or > 50) errors.Add("League points table or fastest lap bonus is invalid.");
        }
        catch { errors.Add("League points table must contain between 1 and 50 integer point values."); }
        return errors;
    }
    private static int Score(int[] table, int position) => position <= table.Length ? table[position - 1] : 0;
    public Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default) => results.GetByRaceAsync(raceId, cancellationToken);
    public Task<PagedResponse<Result>> GetByRacePageAsync(int raceId, PaginationParameters parameters, CancellationToken cancellationToken = default) => results.GetByRacePageAsync(raceId, parameters, cancellationToken);
    public async Task<Result> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await results.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Result with id {id} not found.");
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task DeleteForRaceAsync(int raceId, CancellationToken cancellationToken = default) => ClearClassificationAsync(raceId, cancellationToken);
    public Task DeleteForRegistrationAsync(int registrationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}
