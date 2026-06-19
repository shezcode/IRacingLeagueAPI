using IRacingLeague.Models;

namespace IRacingLeague.Business;

public interface IResultService
{
    Result ApplyResult(Registration registration, Race race, Result result);

    IEnumerable<Result> GetByRace(int raceId);

    Result GetById(int id);

    // Remove a single result, reversing its contribution to the registration's standings
    // points and the driver's global stats.
    void Delete(int id);

    // Cascade helpers: remove a race's / registration's results, reversing each result's
    // contribution to the registration's standings points and the driver's global stats.
    void DeleteForRace(int raceId);

    void DeleteForRegistration(int registrationId);
}
