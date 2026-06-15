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
        var user = _users.Get(registration.UserId)
            ?? throw new KeyNotFoundException($"User with id {registration.UserId} not found.");

        // At most one result per driver per race: re-entering updates in place
        // instead of inserting a duplicate that would double-count points and stats.
        var existingId = _results.GetAll()
            .FirstOrDefault(r => r.RegistrationId == registration.RegistrationId && r.RaceId == race.RaceId)
            ?.ResultId;

        if (existingId is not null)
        {
            // Re-fetch tracked (Find) so a result the caller already loaded resolves to the
            // same instance rather than a second copy that would collide on its key.
            var existing = _results.Get(existingId.Value)!;

            // Back out the previous contribution before applying the new values.
            registration.AddPoints(result.Points - existing.Points);
            user.UndoRaceOutcome(existing.Position, existing.IncidentPoints);
            user.ApplyRaceOutcome(result.Position, result.IncidentPoints);

            // Overwrite the existing row in place.
            existing.Position = result.Position;
            existing.FastestLapSeconds = result.FastestLapSeconds;
            existing.Points = result.Points;
            existing.IncidentPoints = result.IncidentPoints;
            existing.Dnf = result.Dnf;
            existing.Notes = result.Notes;
            existing.FinishedAt = result.FinishedAt;
            _results.Update(existing);
            result = existing;
        }
        else
        {
            // First result for this driver/race: persist it and apply the contribution.
            result.RegistrationId = registration.RegistrationId;
            result.RaceId = race.RaceId;
            _results.Add(result);

            registration.AddPoints(result.Points);
            user.ApplyRaceOutcome(result.Position, result.IncidentPoints);
        }
        _results.SaveChanges();

        _registrations.Update(registration);
        _registrations.SaveChanges();

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
}
