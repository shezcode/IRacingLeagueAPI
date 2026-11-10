using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IStandingsService
{
    Task<PagedResponse<StandingEntry>> GetStandingsAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default);
}
