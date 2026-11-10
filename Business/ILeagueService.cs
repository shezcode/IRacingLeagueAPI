using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public interface ILeagueService
{
    League Create(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId = 0);
    // Public zone with filter/sort applied over IQueryable before materializing (Step 19).
    IEnumerable<League> GetPublic(LeagueQueryParameters parameters);
    League GetById(int id);
    void Update(League league);
    void Delete(int id);
}
