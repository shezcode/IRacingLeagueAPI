using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class RegistrationCreateDTO
{
    [Range(0, 999)]
    public int CarNumber { get; set; }

    [Required, StringLength(60)]
    public string TeamName { get; set; } = string.Empty;

    [Range(0, 200)]
    public decimal BallastKg { get; set; }
}
