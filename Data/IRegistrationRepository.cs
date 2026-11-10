using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRegistrationRepository
{
    void Add(Registration registration);
    Registration? Get(int id);
    IEnumerable<Registration> GetAll();
    void Update(Registration registration);
    void Delete(int id);
    void SaveChanges();
    Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
}
