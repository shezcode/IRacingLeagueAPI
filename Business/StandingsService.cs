using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;
public sealed class StandingsService(IRegistrationRepository registrations, IResultRepository results) : IStandingsService
{
    public async Task<PagedResponse<StandingEntry>> GetStandingsAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var members = await registrations.GetStandingsPageAsync(leagueId, parameters, cancellationToken);
        var rows = new List<StandingEntry>(members.Items.Count);
        foreach (var member in members.Items)
        {
            var memberResults = await results.GetByRegistrationAsync(member.RegistrationId, cancellationToken);
            rows.Add(new StandingEntry(member, memberResults.Count));
        }
        return new PagedResponse<StandingEntry>(rows, members.Page, members.PageSize, members.TotalCount);
    }
}
