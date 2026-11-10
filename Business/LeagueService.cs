using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class LeagueService(ILeagueRepository repository, IRegistrationService registrations, IRaceService races, IRaceRepository raceRepository, IUnitOfWork unitOfWork) : ILeagueService
{
    public async Task<League> CreateAsync(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId = 0, CancellationToken cancellationToken = default)
    { var league = new League(name, discipline, isPublic, maxDrivers, entryFee, ownerUserId); await repository.AddAsync(league, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); return league; }
    public Task<PagedResponse<League>> GetPublicAsync(LeagueQueryParameters parameters, CancellationToken cancellationToken = default) => repository.GetPublicAsync(parameters, cancellationToken);
    public Task<PagedResponse<League>> GetForUserAsync(int userId, PaginationParameters parameters, CancellationToken cancellationToken = default) => repository.GetForUserAsync(userId, parameters, cancellationToken);
    public async Task<League> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await repository.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"League with id {id} not found.");
    public async Task UpdateAsync(League league, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await repository.LockForUpdateAsync(league.LeagueId, ct);
            var current = await repository.GetAsync(league.LeagueId, ct) ?? throw new NotFoundException($"League with id {league.LeagueId} not found.");
            if (league.MaxDrivers < await repository.CountMembersAsync(league.LeagueId, ct)) throw new ConflictException("MaxDrivers cannot be lower than the current membership count.");
            current.Name = league.Name; current.Discipline = league.Discipline; current.IsPublic = league.IsPublic; current.MaxDrivers = league.MaxDrivers; current.EntryFee = league.EntryFee;
            await repository.UpdateAsync(current, ct);
        }, cancellationToken);
    }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"League with id {id} not found.");
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var member in await registrations.GetByLeagueAsync(id, ct)) await registrations.DeleteAsync(member.RegistrationId, ct);
            foreach (var race in await raceRepository.GetScheduledByLeagueAsync(id, ct)) await races.DeleteAsync(race.RaceId, ct);
            await repository.DeleteAsync(id, ct);
        }, cancellationToken);
    }
}
