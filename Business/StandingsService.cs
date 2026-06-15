using IRacingLeague.Data;
using IRacingLeague.Models;

namespace IRacingLeague.Business;

public class StandingsService : IStandingsService
{
    private readonly IRegistrationRepository _registrations;
    private readonly IResultRepository _results;

    public StandingsService(IRegistrationRepository registrations, IResultRepository results)
    {
        _registrations = registrations;
        _results = results;
    }

    public IEnumerable<StandingEntry> GetStandings(int leagueId)
    {
        // Incident points and race counts both live on Result, so accumulate them
        // per registration in a single pass to avoid an inner scan per driver.
        var statsByRegistration = _results.GetAll()
            .GroupBy(r => r.RegistrationId)
            .ToDictionary(
                g => g.Key,
                g => (Incidents: g.Sum(r => r.IncidentPoints), RacesCompleted: g.Count()));

        return _registrations.GetAll()
            .Where(r => r.LeagueId == leagueId)
            .OrderByDescending(r => r.Points)
            .ThenBy(r => statsByRegistration.TryGetValue(r.RegistrationId, out var s) ? s.Incidents : 0)
            .Select(r => new StandingEntry(
                r,
                statsByRegistration.TryGetValue(r.RegistrationId, out var s) ? s.RacesCompleted : 0))
            .ToList();
    }
}
