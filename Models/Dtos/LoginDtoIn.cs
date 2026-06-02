using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class LoginDtoIn
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
