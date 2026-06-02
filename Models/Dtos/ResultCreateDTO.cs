using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class ResultCreateDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "RegistrationId must reference an existing registration.")]
    public int RegistrationId { get; set; }

    [Range(1, 999)]
    public int Position { get; set; }

    [Range(0, 9999)]
    public decimal FastestLapSeconds { get; set; }

    [Range(0, 1000)]
    public int Points { get; set; }

    [Range(0, 100)]
    public int IncidentPoints { get; set; }

    public bool Dnf { get; set; }

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}
