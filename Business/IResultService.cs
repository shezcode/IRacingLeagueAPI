using IRacingLeague.Models;

namespace IRacingLeague.Business;

public interface IResultService
{
    Result ApplyResult(Registration registration, Race race, Result result);

    IEnumerable<Result> GetByRace(int raceId);

    Result GetById(int id);

    void Update(Result result);
}
