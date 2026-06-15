using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public interface IRaceService
{
    Race Create(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC);
    IEnumerable<Race> GetByLeague(int leagueId);
    // A league's races with filter/sort applied over IQueryable before materializing (Step 19).
    IEnumerable<Race> GetByLeague(int leagueId, RaceQueryParameters parameters);
    Race GetById(int id);
    void Update(Race race);
    void Delete(int id);
}
