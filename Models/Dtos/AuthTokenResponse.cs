namespace IRacingLeague.Models.Dtos;

public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);
