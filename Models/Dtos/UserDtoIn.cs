using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class UserDtoIn
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(20)]
    public string Tag { get; set; } = string.Empty;

    [StringLength(10)]
    public string LicenseClass { get; set; } = "Rookie";
}
