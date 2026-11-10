using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Data;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> QueryAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
