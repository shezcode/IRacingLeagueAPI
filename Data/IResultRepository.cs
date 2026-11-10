using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IResultRepository
{
    void Add(Result result);
    Result? Get(int id);
    IEnumerable<Result> GetAll();
    void Update(Result result);
    void Delete(int id);
    void SaveChanges();
    Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRegistrationAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<Result?> FindForRaceAndRegistrationAsync(int raceId, int registrationId, CancellationToken cancellationToken = default);
}
