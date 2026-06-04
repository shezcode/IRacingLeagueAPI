using System.Security.Claims;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public interface IAuthService
{
    string Register(UserDtoIn userDtoIn);

    string Login(LoginDtoIn loginDtoIn);

    string GenerateToken(UserDTOOut user);

    string HashPassword(string password);

    int? GetUserId(ClaimsPrincipal user);

    bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user);
}
