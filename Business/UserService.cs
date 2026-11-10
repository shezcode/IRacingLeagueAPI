using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class UserService(IUserRepository repository, ILeagueRepository leagues, IRegistrationRepository registrations, IRatingReplayRepository replayRepository, IRatingReplayService ratingReplay, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<User> CreateAsync(string userName, string email, string password, string role, string tag, CancellationToken cancellationToken = default)
    { var user = new User(userName, email, PasswordHasher.Hash(password), role, tag, "Rookie"); await repository.AddAsync(user, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); return user; }
    public Task<PagedResponse<User>> QueryAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default) => repository.QueryAsync(parameters, cancellationToken);
    public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await repository.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"User with id {id} not found.");
    public async Task<DriverStatsDTO> GetDriverStatsAsync(int id, int? viewerUserId, bool canViewAllPrivate, CancellationToken cancellationToken = default)
    {
        _ = await GetByIdAsync(id, cancellationToken);
        return await repository.GetDriverStatsAsync(id, viewerUserId, canViewAllPrivate, cancellationToken);
    }
    public async Task UpdateProfileAsync(int id, string userName, string tag, CancellationToken cancellationToken = default)
    { var user = await GetByIdAsync(id, cancellationToken); user.UserName = userName; user.Tag = tag; await repository.UpdateAsync(user, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"User with id {id} not found.");
        var ownedCount = await leagues.CountOwnedByAsync(id, cancellationToken); if (ownedCount > 0) throw new UserOwnsLeaguesException($"User #{id} still owns {ownedCount} league(s). Delete or reassign them before deleting the user.");
        var memberships = await registrations.GetByUserAsync(id, cancellationToken);
        var registrationIds = memberships.Select(x => x.RegistrationId).ToArray();
        var firstAffectedRace = await replayRepository.GetEarliestRaceForRegistrationsAsync(registrationIds, cancellationToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await repository.DeleteAsync(id, ct);
            await unitOfWork.SaveChangesAsync(ct);
            if (firstAffectedRace is not null)
                await ratingReplay.ReplayFromAsync(firstAffectedRace.ScheduledAt, firstAffectedRace.RaceId, registrationIds, ct, [id]);
        }, cancellationToken);
    }
}
