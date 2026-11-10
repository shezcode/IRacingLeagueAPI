using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IStandingsService
{
    Task<IReadOnlyList<StandingEntry>> GetStandingsAsync(int leagueId, CancellationToken cancellationToken = default);
}
