using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILeagueRepository _leagues;
    private readonly IRegistrationService _registrations;

    public UserService(IUserRepository repository, ILeagueRepository leagues, IRegistrationService registrations)
    {
        _repository = repository;
        _leagues = leagues;
        _registrations = registrations;
    }

    public User Create(string userName, string email, string password, string role, string tag, string licenseClass)
    {
        // Persist the hash, never the raw password, so the account can authenticate
        // against AuthService.Login (which compares hashes).
        var user = new User(userName, email, PasswordHasher.Hash(password), role, tag, licenseClass);
        _repository.Add(user);
        _repository.SaveChanges();
        return user;
    }

    public IEnumerable<User> GetAll() => _repository.GetAll();

    // Filter (min iRating, exact license class) + sort applied over IQueryable in SQL.
    public IEnumerable<User> Query(UserQueryParameters p)
    {
        var q = _repository.Query();

        if (p.MinIRating.HasValue)
            q = q.Where(u => u.IRating >= p.MinIRating.Value);
        if (!string.IsNullOrWhiteSpace(p.LicenseClass))
            q = q.Where(u => u.LicenseClass == p.LicenseClass);   // case-insensitive via DB collation

        q = p.SortBy?.ToLowerInvariant() switch
        {
            "totalwins" => p.Desc ? q.OrderByDescending(u => u.TotalWins) : q.OrderBy(u => u.TotalWins),
            "username"  => p.Desc ? q.OrderByDescending(u => u.UserName)  : q.OrderBy(u => u.UserName),
            _           => p.Desc ? q.OrderByDescending(u => u.IRating)   : q.OrderBy(u => u.IRating),
        };

        return q.ToList();
    }

    // Single-field search: drivers whose Tag contains the term (case-insensitive).
    public IEnumerable<User> SearchByTag(string tag) =>
        _repository.GetAll()
            .Where(u => u.Tag.Contains(tag, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public User GetById(int id)
    {
        var user = _repository.Get(id);
        if (user == null)
            throw new KeyNotFoundException($"User with id {id} not found.");
        return user;
    }

    public void Update(User user)
    {
        if (_repository.Get(user.UserId) == null)
            throw new KeyNotFoundException($"User with id {user.UserId} not found.");
        _repository.Update(user);
        _repository.SaveChanges();
    }

    public void Delete(int id)
    {
        if (_repository.Get(id) == null)
            throw new KeyNotFoundException($"User with id {id} not found.");

        // Owned leagues hold other drivers' registrations and results, so we never wipe them
        // as a side effect of deleting their owner. Refuse until the admin clears them first.
        var ownedCount = _leagues.GetAll().Count(l => l.OwnerUserId == id);
        if (ownedCount > 0)
            throw new UserOwnsLeaguesException(
                $"User #{id} still owns {ownedCount} league(s). Delete or reassign them before deleting the user.");

        // Cascade the user's own race entries (and their results, reversing stat contributions);
        // the FK is Restrict, so the user row can only go once these are cleared.
        foreach (var registration in _registrations.GetByUser(id).ToList())
            _registrations.Delete(registration.RegistrationId);

        _repository.Delete(id);
        _repository.SaveChanges();
    }
}
