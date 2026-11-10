using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public sealed class CsvClassificationParser
{
    private static readonly string[] RequiredHeaders = ["position", "car number", "fastest lap", "incidents", "dnf", "notes"];

    public (IReadOnlyList<ClassificationEntryDTO> Entries, IReadOnlyList<string> Errors) Parse(
        TextReader input, IReadOnlyDictionary<int, int> registrationIdsByCarNumber)
    {
        var entries = new List<ClassificationEntryDTO>();
        var errors = new List<string>();
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        };

        try
        {
            using var csv = new CsvReader(input, configuration);
            if (!csv.Read()) return (entries, ["Row 1: CSV header is required."]);
            csv.ReadHeader();
            var headers = csv.HeaderRecord?.Select(x => x.Trim().ToLowerInvariant()).ToHashSet(StringComparer.Ordinal) ?? [];
            foreach (var header in RequiredHeaders)
                if (!headers.Contains(header)) errors.Add($"Row 1, field '{header}': required header is missing.");
            if (errors.Count > 0) return (entries, errors);

            var dataRows = 0;
            var rows = new List<(int Row, int Position, bool Dnf)>();
            var seenCars = new HashSet<int>();
            var seenPositions = new HashSet<int>();
            while (csv.Read())
            {
                dataRows++;
                var row = csv.Parser.Row;
                if (dataRows == 101) errors.Add($"Row {row}: CSV may contain at most 100 data rows.");
                var rowErrors = new List<string>();
                var position = ParseInt(csv.GetField("position"), row, "position", rowErrors);
                var carNumber = ParseInt(csv.GetField("car number"), row, "car number", rowErrors);
                var incidents = ParseInt(csv.GetField("incidents"), row, "incidents", rowErrors);
                var fastestLap = ParseLap(csv.GetField("fastest lap"), row, rowErrors);
                var dnf = ParseDnf(csv.GetField("dnf"), row, rowErrors);
                var notes = csv.GetField("notes") ?? string.Empty;
                if (position is < 1 or > 999) rowErrors.Add($"Row {row}, field 'position': must be between 1 and 999.");
                if (carNumber is < 0 or > 999) rowErrors.Add($"Row {row}, field 'car number': must be between 0 and 999.");
                if (incidents is < 0 or > 100) rowErrors.Add($"Row {row}, field 'incidents': must be between 0 and 100.");
                if (fastestLap is < 0 or > 9999) rowErrors.Add($"Row {row}, field 'fastest lap': must be between 0 and 9999 seconds.");
                if (notes.Length > 500) rowErrors.Add($"Row {row}, field 'notes': must be 500 characters or fewer.");

                if (carNumber.HasValue)
                {
                    if (!registrationIdsByCarNumber.ContainsKey(carNumber.Value)) rowErrors.Add($"Row {row}, field 'car number': no registration in this league uses car number {carNumber.Value}.");
                    if (!seenCars.Add(carNumber.Value)) rowErrors.Add($"Row {row}, field 'car number': car number {carNumber.Value} is duplicated.");
                }
                if (position.HasValue && !seenPositions.Add(position.Value)) rowErrors.Add($"Row {row}, field 'position': position {position.Value} is duplicated.");
                if (rowErrors.Count == 0)
                {
                    rows.Add((row, position!.Value, dnf!.Value));
                    entries.Add(new ClassificationEntryDTO
                    {
                        RegistrationId = registrationIdsByCarNumber[carNumber!.Value],
                        Position = position!.Value,
                        FastestLapSeconds = fastestLap!.Value,
                        IncidentPoints = incidents!.Value,
                        Dnf = dnf!.Value,
                        Notes = notes
                    });
                }
                errors.AddRange(rowErrors);
            }
            if (dataRows > 100) entries.Clear();
            var positions = rows.Select(x => x.Position).Order().ToArray();
            if (positions.Length > 0 && !positions.SequenceEqual(Enumerable.Range(1, positions.Length)))
                foreach (var row in rows.Where(x => x.Position > positions.Length))
                    errors.Add($"Row {row.Row}, field 'position': positions must be consecutive starting at 1.");
            var finishers = rows.Where(x => !x.Dnf).ToArray();
            foreach (var dnf in rows.Where(x => x.Dnf && finishers.Any(finisher => finisher.Position > x.Position)))
                errors.Add($"Row {dnf.Row}, field 'DNF': a DNF must be placed after every finisher.");
        }
        catch (CsvHelperException exception)
        {
            errors.Add($"Row {exception.Context?.Parser?.Row ?? 1}: CSV could not be parsed: {exception.Message}");
        }

        return (entries, errors);
    }

    private static int? ParseInt(string? value, int row, string field, List<string> errors)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)) return parsed;
        errors.Add($"Row {row}, field '{field}': must be an integer.");
        return null;
    }

    private static decimal? ParseLap(string? value, int row, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0m;
        if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds)) return seconds;
        var parts = value.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var minutes) &&
            decimal.TryParse(parts[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var remaining) && remaining is >= 0 and < 60)
            return minutes * 60m + remaining;
        errors.Add($"Row {row}, field 'fastest lap': use seconds, minutes:seconds, or leave it empty.");
        return null;
    }

    private static bool? ParseDnf(string? value, int row, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        errors.Add($"Row {row}, field 'dnf': use true, false, 1, 0, or leave it empty.");
        return null;
    }
}
