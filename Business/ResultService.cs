using IRacingLeague.Data;
using IRacingLeague.Models;

namespace IRacingLeague.Business;

public class ResultService : IResultService
{
    private readonly IResultRepository _results;
    private readonly IRegistrationRepository _registrations;
    private readonly IUserRepository _users;

    public ResultService(IResultRepository results, IRegistrationRepository registrations, IUserRepository users)
    {
        _results = results;
        _registrations = registrations;
        _users = users;
    }

    public Result ApplyResult(Registration registration, Race race, Result result)
    {
        // 1. Persist the result, bound to its registration and race.
        result.RegistrationId = registration.RegistrationId;
        result.RaceId = race.RaceId;
        _results.Add(result);
        _results.SaveChanges();

        // 2. Add the race points to this league standing.
        registration.AddPoints(result.Points);
        _registrations.Update(registration);
        _registrations.SaveChanges();

        // 3. Recompute the owning user's global stats.
        var user = _users.Get(registration.UserId)
            ?? throw new KeyNotFoundException($"User with id {registration.UserId} not found.");
        user.ApplyRaceOutcome(result.Position, result.IncidentPoints);
        _users.Update(user);
        _users.SaveChanges();

        return result;
    }

    public IEnumerable<Result> GetByRace(int raceId) =>
        _results.GetAll().Where(r => r.RaceId == raceId).OrderBy(r => r.Position).ToList();

    public Result GetById(int id)
    {
        var result = _results.Get(id);
        if (result == null)
            throw new KeyNotFoundException($"Result with id {id} not found.");
        return result;
    }

    // Field-level correction only, see IResultService.Update for the standings caveat.
    public void Update(Result result)
    {
        if (_results.Get(result.ResultId) == null)
            throw new KeyNotFoundException($"Result with id {result.ResultId} not found.");
        _results.Update(result);
        _results.SaveChanges();
    }
}
