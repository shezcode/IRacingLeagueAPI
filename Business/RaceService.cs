using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class RaceService(IRaceRepository races, ILeagueRepository leagues, IResultRepository results, IRatingReplayRepository replayRepository, IRatingReplayService ratingReplay, IUnitOfWork unitOfWork) : IRaceService
{
    public async Task<Race> CreateAsync(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC, CancellationToken cancellationToken = default)
    {
        if (await leagues.GetAsync(leagueId, cancellationToken) is null) throw new NotFoundException($"League with id {leagueId} not found.");
        var race = new Race(leagueId, track, car, AsUtc(scheduledAt), lapCount, ambientTempC);
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await races.AddAsync(race, ct); await unitOfWork.SaveChangesAsync(ct); await RenumberRoundsAsync(leagueId, ct); }, cancellationToken);
        return race;
    }
    public Task<PagedResponse<Race>> GetByLeagueAsync(int leagueId, RaceQueryParameters parameters, CancellationToken cancellationToken = default) => races.GetByLeagueAsync(leagueId, parameters, cancellationToken);
    public async Task<Race> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await races.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Race with id {id} not found.");
    public async Task UpdateAsync(Race race, CancellationToken cancellationToken = default)
    {
        race.ScheduledAt = AsUtc(race.ScheduledAt);
        var existing = await races.GetAsync(race.RaceId, cancellationToken) ?? throw new NotFoundException($"Race with id {race.RaceId} not found.");
        if (existing.IsCompleted && race.ScheduledAt != existing.ScheduledAt) throw new ConflictException("A completed race cannot be rescheduled.");
        await unitOfWork.ExecuteInTransactionAsync(async ct => { await races.UpdateAsync(race, ct); await RenumberRoundsAsync(race.LeagueId, ct); }, cancellationToken);
    }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var race = await races.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Race with id {id} not found.");
        var deletedResults = await results.GetByRaceAsync(id, cancellationToken);
        var registrationIds = deletedResults.Select(x => x.RegistrationId).Distinct().ToArray();
        var userIds = await replayRepository.GetUserIdsForRegistrationsAsync(registrationIds, cancellationToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await races.DeleteAsync(id, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await RenumberRoundsAsync(race.LeagueId, ct);
            if (deletedResults.Count > 0)
                await ratingReplay.ReplayFromAsync(race.ScheduledAt, race.RaceId, registrationIds, ct, userIds);
        }, cancellationToken);
    }
    private async Task RenumberRoundsAsync(int leagueId, CancellationToken ct)
    {
        var ordered = await races.GetScheduledByLeagueAsync(leagueId, ct);
        for (var i = 0; i < ordered.Count; i++) if (ordered[i].Round != i + 1) { ordered[i].Round = i + 1; await races.UpdateAsync(ordered[i], ct); }
    }
    private static DateTime AsUtc(DateTime value) => value.Kind switch { DateTimeKind.Utc => value, DateTimeKind.Local => value.ToUniversalTime(), _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) };
}
