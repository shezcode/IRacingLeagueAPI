namespace IRacingLeague.Models.Dtos;

public sealed record LeagueInviteCreatedDTO(string Token, DateTime CreatedAt, DateTime ExpiresAt);
