using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public interface IUserService
{
    User Create(string userName, string email, string password, string role, string tag, string licenseClass);
    IEnumerable<User> Query(UserQueryParameters parameters);
    User GetById(int id);
    void Update(User user);
    void Delete(int id);
}
