using IRacingLeague.Data;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public sealed class StandingsService(IRegistrationRepository registrations, IResultRepository results) : IStandingsService
{
    public async Task<IReadOnlyList<StandingEntry>> GetStandingsAsync(int leagueId, CancellationToken cancellationToken = default)
    {
        var members = await registrations.GetByLeagueAsync(leagueId, cancellationToken);
        var rows = new List<StandingEntry>(members.Count);
        foreach (var member in members)
        {
            var memberResults = await results.GetByRegistrationAsync(member.RegistrationId, cancellationToken);
            rows.Add(new StandingEntry(member, memberResults.Count));
        }
        return rows.OrderByDescending(x => x.Registration.Points).ToArray();
    }
}
