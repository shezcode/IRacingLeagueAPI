using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
namespace IRacingLeague.Business;

public class AuthService(IConfiguration configuration, IUserRepository users, IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<string> RegisterAsync(UserDtoIn dto, CancellationToken cancellationToken = default)
    {
        if (await users.EmailExistsAsync(dto.Email, cancellationToken)) throw new ConflictException($"An account already exists for '{dto.Email}'.");
        var user = new User(dto.UserName, dto.Email, HashPassword(dto.Password), Roles.Driver.ToString(), dto.Tag, "Rookie");
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return GenerateToken(UserDTOOut.FromEntity(user));
    }
    public async Task<string> LoginAsync(LoginDtoIn dto, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByEmailAsync(dto.Email, cancellationToken);
        if (user is null || user.Password != HashPassword(dto.Password)) throw new UnauthorizedAccessException("Invalid email or password.");
        return GenerateToken(UserDTOOut.FromEntity(user));
    }
    public string GenerateToken(UserDTOOut user)
    {
        var secret = configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
        var key = Encoding.UTF8.GetBytes(secret);
        var descriptor = new SecurityTokenDescriptor { Issuer = configuration["JWT:ValidIssuer"], Audience = configuration["JWT:ValidAudience"], Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.Email, user.Email) }), Expires = DateTime.UtcNow.AddDays(7), SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) };
        var handler = new JwtSecurityTokenHandler(); return handler.WriteToken(handler.CreateToken(descriptor));
    }
    public string HashPassword(string password) => PasswordHasher.Hash(password);
    public int? GetUserId(ClaimsPrincipal user) { var claim = user.FindFirst(ClaimTypes.NameIdentifier); return claim is not null && int.TryParse(claim.Value, out var id) ? id : null; }
    public bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user) => GetUserId(user) == ownerUserId || user.IsInRole(Roles.Admin.ToString());
}
