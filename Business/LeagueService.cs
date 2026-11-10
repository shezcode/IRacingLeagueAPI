using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class LeagueService(ILeagueRepository repository, IRegistrationRepository registrations, IResultRepository results, IRatingReplayRepository replayRepository, IRatingReplayService ratingReplay, IUnitOfWork unitOfWork) : ILeagueService
{
    public async Task<League> CreateAsync(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId = 0, CancellationToken cancellationToken = default, int[]? pointsTable = null, int? fastestLapBonus = null)
    {
        var selectedPoints = pointsTable ?? [25, 18, 15, 12, 10, 8, 6, 4, 2, 1];
        var selectedBonus = fastestLapBonus ?? 1;
        var scoringErrors = LeagueScoring.Validate(selectedPoints, selectedBonus);
        if (scoringErrors.Count > 0) throw new DomainValidationException(string.Join("; ", scoringErrors));
        var league = new League(name, discipline, isPublic, maxDrivers, entryFee, ownerUserId)
        { PointsTable = selectedPoints, FastestLapBonus = selectedBonus };
        await repository.AddAsync(league, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return league;
    }
    public Task<PagedResponse<League>> GetPublicAsync(LeagueQueryParameters parameters, CancellationToken cancellationToken = default) => repository.GetPublicAsync(parameters, cancellationToken);
    public Task<PagedResponse<League>> GetForUserAsync(int userId, PaginationParameters parameters, CancellationToken cancellationToken = default) => repository.GetForUserAsync(userId, parameters, cancellationToken);
    public async Task<League> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await repository.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"League with id {id} not found.");
    public async Task UpdateAsync(League league, int[]? pointsTable = null, int? fastestLapBonus = null, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await repository.LockForUpdateAsync(league.LeagueId, ct);
            var current = await repository.GetAsync(league.LeagueId, ct) ?? throw new NotFoundException($"League with id {league.LeagueId} not found.");
            if (league.MaxDrivers < await repository.CountMembersAsync(league.LeagueId, ct)) throw new ConflictException("MaxDrivers cannot be lower than the current membership count.");
            var nextPointsTable = pointsTable ?? current.PointsTable;
            var nextFastestLapBonus = fastestLapBonus ?? current.FastestLapBonus;
            var scoringErrors = LeagueScoring.Validate(nextPointsTable, nextFastestLapBonus);
            if (scoringErrors.Count > 0) throw new DomainValidationException(string.Join("; ", scoringErrors));
            var scoringChanged = !current.PointsTable.SequenceEqual(nextPointsTable) || current.FastestLapBonus != nextFastestLapBonus;
            current.Name = league.Name; current.Discipline = league.Discipline; current.IsPublic = league.IsPublic; current.MaxDrivers = league.MaxDrivers; current.EntryFee = league.EntryFee;
            current.PointsTable = nextPointsTable;
            current.FastestLapBonus = nextFastestLapBonus;
            await repository.UpdateAsync(current, ct);
            if (scoringChanged)
            {
                var allResults = await results.GetByLeagueAsync(current.LeagueId, ct);
                foreach (var raceResults in allResults.GroupBy(x => x.RaceId))
                {
                    var fastest = raceResults.Where(x => !x.Dnf && x.FastestLapSeconds > 0)
                        .OrderBy(x => x.FastestLapSeconds).ThenBy(x => x.Position).FirstOrDefault();
                    foreach (var result in raceResults)
                    {
                        result.FastestLap = fastest?.ResultId == result.ResultId;
                        result.Points = result.Dnf ? 0 :
                            (result.Position <= nextPointsTable.Length ? nextPointsTable[result.Position - 1] : 0) +
                            (result.FastestLap ? nextFastestLapBonus : 0);
                        await results.UpdateAsync(result, ct);
                    }
                }
            }
        }, cancellationToken);
    }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"League with id {id} not found.");
        var members = await registrations.GetByLeagueAsync(id, cancellationToken);
        var registrationIds = members.Select(x => x.RegistrationId).ToArray();
        var userIds = members.Select(x => x.UserId).Distinct().ToArray();
        var firstAffectedRace = await replayRepository.GetEarliestRaceForRegistrationsAsync(registrationIds, cancellationToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await repository.DeleteAsync(id, ct);
            await unitOfWork.SaveChangesAsync(ct);
            if (firstAffectedRace is not null)
                await ratingReplay.ReplayFromAsync(firstAffectedRace.ScheduledAt, firstAffectedRace.RaceId, registrationIds, ct, userIds);
        }, cancellationToken);
    }
}
