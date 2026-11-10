using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public interface IUserService
{
    Task<User> CreateAsync(string userName, string email, string password, string role, string tag, CancellationToken cancellationToken = default);
    Task<PagedResponse<User>> QueryAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(int id, string userName, string tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
