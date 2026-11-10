using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class UserService(IUserRepository repository, ILeagueRepository leagues, IRegistrationService registrations, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<User> CreateAsync(string userName, string email, string password, string role, string tag, string licenseClass, CancellationToken cancellationToken = default)
    { var user = new User(userName, email, PasswordHasher.Hash(password), role, tag, licenseClass); await repository.AddAsync(user, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); return user; }
    public Task<IReadOnlyList<User>> QueryAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default) => repository.QueryAsync(parameters, cancellationToken);
    public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await repository.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"User with id {id} not found.");
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    { if (await repository.GetAsync(user.UserId, cancellationToken) is null) throw new NotFoundException($"User with id {user.UserId} not found."); await repository.UpdateAsync(user, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"User with id {id} not found.");
        var ownedCount = await leagues.CountOwnedByAsync(id, cancellationToken); if (ownedCount > 0) throw new UserOwnsLeaguesException($"User #{id} still owns {ownedCount} league(s). Delete or reassign them before deleting the user.");
        await unitOfWork.ExecuteInTransactionAsync(async ct => { foreach (var registration in await registrations.GetByUserAsync(id, ct)) await registrations.DeleteAsync(registration.RegistrationId, ct); await repository.DeleteAsync(id, ct); }, cancellationToken);
    }
}
