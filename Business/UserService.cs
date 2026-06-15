using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public User Create(string userName, string email, string password, string role, string tag, string licenseClass)
    {
        var user = new User(userName, email, password, role, tag, licenseClass);
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
        _repository.Delete(id);
        _repository.SaveChanges();
    }
}
