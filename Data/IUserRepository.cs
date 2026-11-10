using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(User user);
    User? Get(int id);
    IEnumerable<User> GetAll();
    // Composable query seam for service-layer filter/sort over IQueryable (spec §"filter/sort").
    IQueryable<User> Query();
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(int id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
