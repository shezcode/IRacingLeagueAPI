using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IRacingLeague.Business;

public class AuthService(IConfiguration configuration, IUserRepository users, IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork) : IAuthService
{
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(14);

    public async Task<AuthTokenResponse> RegisterAsync(UserDtoIn dto, CancellationToken cancellationToken = default)
    {
        if (await users.EmailExistsAsync(dto.Email, cancellationToken)) throw new ConflictException($"An account already exists for '{dto.Email}'.");
        var user = new User(dto.UserName, dto.Email, HashPassword(dto.Password), Roles.Driver.ToString(), dto.Tag, "Rookie");
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, Guid.NewGuid(), cancellationToken);
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginDtoIn dto, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByEmailAsync(dto.Email, cancellationToken);
        if (user is null || !PasswordHasher.Verify(user.Password, dto.Password)) throw new UnauthorizedAccessException("Invalid email or password.");
        return await IssueTokensAsync(user, Guid.NewGuid(), cancellationToken);
    }

    public async Task<AuthTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var rawHash = HashRefreshToken(refreshToken);
        AuthTokenResponse? response = null;
        var reused = false;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var stored = await refreshTokens.FindForUpdateByHashAsync(rawHash, ct);
            if (stored is null) return;
            if (stored.RevokedAt.HasValue)
            {
                await refreshTokens.RevokeFamilyAsync(stored.FamilyId, DateTime.UtcNow, ct);
                reused = true;
                return;
            }
            if (stored.ExpiresAt <= DateTime.UtcNow) return;

            var user = await users.GetAsync(stored.UserId, ct);
            if (user is null) return;
            var replacementPlain = CreateRefreshSecret();
            var replacementHash = HashRefreshToken(replacementPlain);
            var now = DateTime.UtcNow;
            stored.RevokedAt = now;
            stored.ReplacedByTokenHash = replacementHash;
            await refreshTokens.AddAsync(new RefreshToken { UserId = user.UserId, FamilyId = stored.FamilyId, TokenHash = replacementHash, CreatedAt = now, ExpiresAt = now.Add(RefreshLifetime) }, ct);
            response = CreateResponse(user, replacementPlain, now);
        }, cancellationToken);

        if (reused) throw new UnauthorizedAccessException("Invalid refresh token.");
        return response ?? throw new UnauthorizedAccessException("Invalid refresh token.");
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashRefreshToken(refreshToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var stored = await refreshTokens.FindByHashAsync(hash, ct);
            if (stored is not null) await refreshTokens.RevokeFamilyAsync(stored.FamilyId, DateTime.UtcNow, ct);
        }, cancellationToken);
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(User user, Guid familyId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var plain = CreateRefreshSecret();
        await refreshTokens.AddAsync(new RefreshToken { UserId = user.UserId, FamilyId = familyId, TokenHash = HashRefreshToken(plain), CreatedAt = now, ExpiresAt = now.Add(RefreshLifetime) }, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return CreateResponse(user, plain, now);
    }

    private AuthTokenResponse CreateResponse(User user, string refreshToken, DateTime now) =>
        new(GenerateToken(UserDTOOut.FromEntity(user), now.Add(AccessLifetime)), refreshToken, now.Add(AccessLifetime), now.Add(RefreshLifetime));

    private string GenerateToken(UserDTOOut user, DateTime expiresAt)
    {
        var secret = configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
        var descriptor = new SecurityTokenDescriptor { Issuer = configuration["JWT:ValidIssuer"], Audience = configuration["JWT:ValidAudience"], Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.Email, user.Email) }), Expires = expiresAt, SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature) };
        var handler = new JwtSecurityTokenHandler(); return handler.WriteToken(handler.CreateToken(descriptor));
    }

    private static string CreateRefreshSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    public string HashPassword(string password) => PasswordHasher.Hash(password);
    public int? GetUserId(ClaimsPrincipal user) { var claim = user.FindFirst(ClaimTypes.NameIdentifier); return claim is not null && int.TryParse(claim.Value, out var id) ? id : null; }
    public bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user) => GetUserId(user) == ownerUserId || user.IsInRole(Roles.Admin.ToString());
}
