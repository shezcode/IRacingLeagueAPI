using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class UserUpdateDTO
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [StringLength(20)]
    public string Tag { get; set; } = string.Empty;

    [StringLength(10)]
    public string LicenseClass { get; set; } = "Rookie";
}
