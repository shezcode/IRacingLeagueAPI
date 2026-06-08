using System.Security.Cryptography;
using System.Text;
using IRacingLeague.Models;

namespace IRacingLeague.Data.EF;

// Populates a freshly-migrated database with starter data so the app isn't empty
// on first run. No-op if any users already exist.
public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var admin = new User("admin", "admin@iracingleague.com", HashPassword("Admin123!"), Roles.Admin, "ADM", "A")
        {
            IRating = 5000,
            SafetyRating = 4.99m
        };
        var max = new User("Max Verstappen", "max@example.com", HashPassword("Driver123!"), Roles.Driver, "VER", "A")
        {
            IRating = 4200,
            SafetyRating = 4.20m
        };
        var lewis = new User("Lewis Hamilton", "lewis@example.com", HashPassword("Driver123!"), Roles.Driver, "HAM", "A")
        {
            IRating = 4100,
            SafetyRating = 4.50m
        };
        var charles = new User("Charles Leclerc", "charles@example.com", HashPassword("Driver123!"), Roles.Driver, "LEC", "B")
        {
            IRating = 3500,
            SafetyRating = 3.80m
        };
        var lando = new User("Lando Norris", "lando@example.com", HashPassword("Driver123!"), Roles.Driver, "NOR", "B")
        {
            IRating = 3300,
            SafetyRating = 4.10m
        };
        var george = new User("George Russell", "george@example.com", HashPassword("Driver123!"), Roles.Driver, "RUS", "C")
        {
            IRating = 2800,
            SafetyRating = 3.95m
        };

        context.Users.AddRange(admin, max, lewis, charles, lando, george);
        context.SaveChanges();

        var gt3League = new League("GT3 Sprint Series", "Road", true, 20, 25.00m, admin.UserId);
        var ovalLeague = new League("Oval Masters", "Oval", false, 16, 0m, admin.UserId);

        context.Leagues.AddRange(gt3League, ovalLeague);
        context.SaveChanges();

        var regMax = new Registration(max.UserId, gt3League.LeagueId, 1, "Red Bull Racing", 0m);
        var regLewis = new Registration(lewis.UserId, gt3League.LeagueId, 44, "Mercedes AMG", 0m);
        var regCharles = new Registration(charles.UserId, gt3League.LeagueId, 16, "Ferrari", 5m);
        var regLando = new Registration(lando.UserId, gt3League.LeagueId, 4, "McLaren", 0m);
        var regGeorge = new Registration(george.UserId, gt3League.LeagueId, 63, "Mercedes AMG", 2.5m);

        context.Registrations.AddRange(regMax, regLewis, regCharles, regLando, regGeorge);

        var raceSpa = new Race(gt3League.LeagueId, "Spa-Francorchamps", "Mercedes AMG GT3",
            new DateTime(2026, 5, 3, 18, 0, 0), 20, 18.5m, 1) { IsCompleted = true };
        var raceNurburgring = new Race(gt3League.LeagueId, "Nurburgring GP", "Ferrari 296 GT3",
            new DateTime(2026, 5, 17, 18, 0, 0), 25, 22.0m, 2) { IsCompleted = true };
        var raceMonza = new Race(gt3League.LeagueId, "Monza", "McLaren 720S GT3",
            new DateTime(2026, 7, 12, 18, 0, 0), 22, 26.0m, 3);

        context.Races.AddRange(raceSpa, raceNurburgring, raceMonza);
        context.SaveChanges();

        // Race 1: Spa-Francorchamps
        AddResult(context, regMax, raceSpa, 1, 123.456m, 25, 0, max);
        AddResult(context, regLewis, raceSpa, 2, 123.789m, 18, 1, lewis);
        AddResult(context, regCharles, raceSpa, 3, 124.012m, 15, 2, charles);
        AddResult(context, regLando, raceSpa, 4, 124.345m, 12, 0, lando);
        AddResult(context, regGeorge, raceSpa, 5, 124.987m, 10, 4, george);

        // Race 2: Nurburgring GP
        AddResult(context, regLewis, raceNurburgring, 1, 118.234m, 25, 0, lewis);
        AddResult(context, regMax, raceNurburgring, 2, 118.456m, 18, 1, max);
        AddResult(context, regLando, raceNurburgring, 3, 119.001m, 15, 0, lando);
        AddResult(context, regGeorge, raceNurburgring, 4, 119.456m, 12, 2, george);
        AddResult(context, regCharles, raceNurburgring, 5, 0m, 0, 5, charles, dnf: true, notes: "Collision at Eau Rouge");

        context.SaveChanges();
    }

    private static void AddResult(AppDbContext context, Registration registration, Race race, int position,
        decimal fastestLapSeconds, int points, int incidentPoints, User driver, bool dnf = false, string notes = "")
    {
        var result = new Result(registration.RegistrationId, race.RaceId, position, fastestLapSeconds,
            points, incidentPoints, dnf, notes);
        context.Results.Add(result);

        registration.AddPoints(points);
        driver.ApplyRaceOutcome(position, incidentPoints);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
