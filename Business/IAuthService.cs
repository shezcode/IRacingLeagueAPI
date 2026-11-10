using System.Security.Claims;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public interface IAuthService
{
    Task<AuthTokenResponse> RegisterAsync(UserDtoIn dto, CancellationToken cancellationToken = default);
    Task<AuthTokenResponse> LoginAsync(LoginDtoIn dto, CancellationToken cancellationToken = default);
    Task<AuthTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    string HashPassword(string password);
    int? GetUserId(ClaimsPrincipal user);
    bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user);
}
