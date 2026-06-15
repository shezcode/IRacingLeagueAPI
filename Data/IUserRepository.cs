using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IUserRepository
{
    void Add(User user);
    User? Get(int id);
    IEnumerable<User> GetAll();
    // Composable query seam for service-layer filter/sort over IQueryable (spec §"filter/sort").
    IQueryable<User> Query();
    void Update(User user);
    void Delete(int id);
    void SaveChanges();
}
