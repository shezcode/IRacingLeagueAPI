using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;
public sealed class StandingsService(IRegistrationRepository registrations) : IStandingsService
{
    public async Task<PagedResponse<StandingEntry>> GetStandingsAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var page = await registrations.GetStandingsAsync(leagueId, parameters, cancellationToken);
        return new PagedResponse<StandingEntry>(page.Items.Select((x, i) => new StandingEntry(x.Registration, x.RacesCompleted, x.Points, x.Wins, x.Podiums, x.IncidentPoints, x.BestFinish, (parameters.Page - 1) * parameters.PageSize + i + 1)).ToArray(), page.Page, page.PageSize, page.TotalCount);
    }
}
