using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRegistrationRepository
{
    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);
    Task<Registration?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Registration registration);
    Registration? Get(int id);
    IEnumerable<Registration> GetAll();
    void Update(Registration registration);
    void Delete(int id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
}
