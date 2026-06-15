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

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _users;

    public AuthService(IConfiguration configuration, IUserRepository users)
    {
        _configuration = configuration;
        _users = users;
    }

    public string Register(UserDtoIn dto)
    {
        // Email is unique; reject duplicates with a friendly error
        // rather than letting the DB throw on SaveChanges.
        if (_users.GetAll().Any(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"An account already exists for '{dto.Email}'.");

        var user = new User(dto.UserName, dto.Email, HashPassword(dto.Password),
                            Roles.Driver, dto.Tag, dto.LicenseClass);
        _users.Add(user);
        _users.SaveChanges();   // populates user.UserId

        return GenerateToken(UserDTOOut.FromEntity(user));
    }

    public string Login(LoginDtoIn dto)
    {
        var user = _users.GetAll()
            .FirstOrDefault(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase));

        // Same error for unknown email and wrong password, so the response can't be used
        // to probe which emails are registered.
        if (user is null || user.Password != HashPassword(dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return GenerateToken(UserDTOOut.FromEntity(user));
    }

    public string GenerateToken(UserDTOOut user)
    {
        var secret = _configuration["JWT:SecretKey"]
            ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
        var key = Encoding.UTF8.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWT:ValidIssuer"],
            Audience = _configuration["JWT:ValidAudience"],
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public int? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out int id) ? id : null;
    }

    public bool HasAccessToResource(int ownerUserId, ClaimsPrincipal user)
    {
        var isOwner = GetUserId(user) == ownerUserId;
        var isAdmin = user.IsInRole(Roles.Admin);
        return isOwner || isAdmin;
    }
}
