using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public sealed class ClassificationEntryDTO
{
    [Range(1, int.MaxValue)] public int RegistrationId { get; set; }
    [Range(1, 999)] public int Position { get; set; }
    [Range(0, 9999)] public decimal FastestLapSeconds { get; set; }
    [Range(0, 100)] public int IncidentPoints { get; set; }
    public bool Dnf { get; set; }
    [StringLength(500)] public string Notes { get; set; } = string.Empty;
}
