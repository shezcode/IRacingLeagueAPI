using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class RaceService(IRaceRepository races, ILeagueRepository leagues, IResultService results, IUnitOfWork unitOfWork) : IRaceService
{
    public async Task<Race> CreateAsync(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC, CancellationToken cancellationToken = default)
    {
        if (await leagues.GetAsync(leagueId, cancellationToken) is null) throw new NotFoundException($"League with id {leagueId} not found.");
        var race = new Race(leagueId, track, car, AsUtc(scheduledAt), lapCount, ambientTempC);
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await races.AddAsync(race, ct); await unitOfWork.SaveChangesAsync(ct); await RenumberRoundsAsync(leagueId, ct); }, cancellationToken);
        return race;
    }
    public Task<IReadOnlyList<Race>> GetByLeagueAsync(int leagueId, RaceQueryParameters parameters, CancellationToken cancellationToken = default) => races.GetByLeagueAsync(leagueId, parameters, cancellationToken);
    public async Task<Race> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await races.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Race with id {id} not found.");
    public async Task UpdateAsync(Race race, CancellationToken cancellationToken = default)
    {
        race.ScheduledAt = AsUtc(race.ScheduledAt);
        if (await races.GetAsync(race.RaceId, cancellationToken) is null) throw new NotFoundException($"Race with id {race.RaceId} not found.");
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await races.UpdateAsync(race, ct); await RenumberRoundsAsync(race.LeagueId, ct); }, cancellationToken);
    }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var race = await races.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Race with id {id} not found.");
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await results.DeleteForRaceAsync(id, ct); await races.DeleteAsync(id, ct); await RenumberRoundsAsync(race.LeagueId, ct); }, cancellationToken);
    }
    private async Task RenumberRoundsAsync(int leagueId, CancellationToken ct)
    {
        var ordered = await races.GetScheduledByLeagueAsync(leagueId, ct);
        for (var i = 0; i < ordered.Count; i++) if (ordered[i].Round != i + 1) { ordered[i].Round = i + 1; await races.UpdateAsync(ordered[i], ct); }
    }
    private static DateTime AsUtc(DateTime value) => value.Kind switch { DateTimeKind.Utc => value, DateTimeKind.Local => value.ToUniversalTime(), _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) };
}
