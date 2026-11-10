using IRacingLeague.Data;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public sealed class RegistrationService(IRegistrationRepository registrations, ILeagueRepository leagues, IResultService results, IUnitOfWork unitOfWork) : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int userId, int leagueId, int carNumber, string teamName, decimal ballastKg, CancellationToken cancellationToken = default)
    {
        var league = await leagues.GetAsync(leagueId, cancellationToken) ?? throw new NotFoundException($"League with id {leagueId} not found.");
        if (await registrations.FindMembershipAsync(userId, leagueId, cancellationToken) is not null) throw new DuplicateRegistrationException($"Driver #{userId} is already registered in league '{league.Name}'.");
        if (await registrations.CountByLeagueAsync(leagueId, cancellationToken) >= league.MaxDrivers) throw new LeagueFullException($"League '{league.Name}' is full ({league.MaxDrivers} drivers).");
        var registration = new Registration(userId, leagueId, carNumber, teamName, ballastKg);
        await registrations.AddAsync(registration, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); return registration;
    }
    public Task<bool> IsMemberAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => registrations.HasMembershipAsync(userId, leagueId, cancellationToken);
    public Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => registrations.GetByLeagueAsync(leagueId, cancellationToken);
    public Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) => registrations.GetByUserAsync(userId, cancellationToken);
    public async Task<Registration> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await registrations.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Registration with id {id} not found.");
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { if (await registrations.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"Registration with id {id} not found."); await unitOfWork.ExecuteInTransactionAsync(async ct => { await results.DeleteForRegistrationAsync(id, ct); await registrations.DeleteAsync(id, ct); }, cancellationToken); }
}
