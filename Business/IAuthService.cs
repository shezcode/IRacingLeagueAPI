using System.Security.Claims;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public interface IAuthService
{
    Task<string> RegisterAsync(UserDtoIn dto, CancellationToken cancellationToken = default);
    Task<string> LoginAsync(LoginDtoIn dto, CancellationToken cancellationToken = default);
    string GenerateToken(UserDTOOut user);
    string HashPassword(string password);
    int? GetUserId(ClaimsPrincipal user);
    bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user);
}
