using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class RegistrationCreateDTO
{
    // Optional target driver. Omitted (or equal to the caller) = self-registration.
    // Registering a different driver is restricted to the league owner ("Add driver").
    public int? UserId { get; set; }

    [Range(0, 999)]
    public int CarNumber { get; set; }

    [Required, StringLength(60)]
    public string TeamName { get; set; } = string.Empty;

    [Range(0, 200)]
    public decimal BallastKg { get; set; }
}
