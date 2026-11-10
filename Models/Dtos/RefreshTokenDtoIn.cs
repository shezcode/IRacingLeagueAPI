using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public sealed class RefreshTokenDtoIn
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
