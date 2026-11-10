using IRacingLeague.Business;
using IRacingLeague.Data;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.API;

public sealed class DemoDataSeeder(
    IUserService users,
    ILeagueService leagues,
    IRegistrationService registrations,
    IRaceService races,
    IResultService results,
    IUserRepository userRepository,
    ILeagueRepository leagueRepository)
{
    private const string DriverPassword = "Demo123!";
    private const string AdminPassword = "Admin123!";

    public async Task SeedIfNeededAsync(bool reset, CancellationToken cancellationToken = default)
    {
        if (reset)
        {
            foreach (var league in await leagueRepository.GetAllAsync(cancellationToken))
                await leagues.DeleteAsync(league.LeagueId, cancellationToken);
            var allUsers = await userRepository.QueryAsync(new UserQueryParameters { Page = 1, PageSize = 100 }, cancellationToken);
            foreach (var user in allUsers.Items)
                await users.DeleteAsync(user.UserId, cancellationToken);
        }
        else if ((await userRepository.QueryAsync(new UserQueryParameters { Page = 1, PageSize = 1 }, cancellationToken)).TotalCount > 0)
        {
            return;
        }

        var admin = await users.CreateAsync("League Admin", "admin@iracingleague.com", AdminPassword, "Admin", "ADM", cancellationToken);
        var drivers = new (string Name, string Email, string Tag)[]
        {
            ("Max Verstappen", "max@example.com", "VER"),
            ("Lewis Hamilton", "lewis@example.com", "HAM"),
            ("Charles Leclerc", "charles@example.com", "LEC"),
            ("Lando Norris", "lando@example.com", "NOR"),
            ("George Russell", "george@example.com", "RUS"),
            ("Carlos Sainz", "carlos@example.com", "SAI"),
            ("Fernando Alonso", "fernando@example.com", "ALO"),
            ("Oscar Piastri", "oscar@example.com", "PIA"),
            ("Sergio Perez", "sergio@example.com", "PER"),
            ("Pierre Gasly", "pierre@example.com", "GAS"),
            ("Yuki Tsunoda", "yuki@example.com", "TSU")
        };
        var driverUsers = new List<IRacingLeague.Models.User>();
        foreach (var driver in drivers)
            driverUsers.Add(await users.CreateAsync(driver.Name, driver.Email, DriverPassword, "Driver", driver.Tag, cancellationToken));

        var publicLeague = await leagues.CreateAsync("GT3 Sprint Series", "Road", true, 24, 25m, admin.UserId, cancellationToken);
        var enduranceLeague = await leagues.CreateAsync("Endurance Cup", "Road", true, 16, 0m, driverUsers[0].UserId, cancellationToken);
        var privateLeague = await leagues.CreateAsync("Paddock Invitational", "Road", false, 12, 0m, driverUsers[1].UserId, cancellationToken);

        var publicMembers = await JoinLeague(publicLeague.LeagueId, driverUsers, null, cancellationToken);
        var enduranceMembers = await JoinLeague(enduranceLeague.LeagueId, driverUsers.Take(8).ToList(), null, cancellationToken);
        var privateMembers = await JoinLeague(privateLeague.LeagueId, driverUsers.Take(6).ToList(), privateLeague.OwnerUserId, cancellationToken);

        await SeedRaces(publicLeague.LeagueId, "Spa-Francorchamps", publicMembers, cancellationToken);
        await SeedRaces(enduranceLeague.LeagueId, "Silverstone", enduranceMembers, cancellationToken);
        await SeedRaces(privateLeague.LeagueId, "Road Atlanta", privateMembers, cancellationToken);
    }

    private async Task<List<IRacingLeague.Models.Registration>> JoinLeague(
        int leagueId, IReadOnlyList<IRacingLeague.Models.User> members, int? inviteCreatorUserId,
        CancellationToken cancellationToken)
    {
        var joined = new List<IRacingLeague.Models.Registration>();
        foreach (var (member, index) in members.Select((user, index) => (user, index)))
        {
            string? inviteToken = null;
            var requireInvite = false;
            if (inviteCreatorUserId.HasValue)
            {
                var invite = await registrations.CreateInviteAsync(leagueId, inviteCreatorUserId.Value, 72, cancellationToken);
                inviteToken = invite.Token;
                requireInvite = true;
            }
            joined.Add(await registrations.RegisterAsync(member.UserId, leagueId, 10 + index, member.UserName,
                0m, inviteToken, requireInvite, cancellationToken));
        }
        return joined;
    }

    private async Task SeedRaces(int leagueId, string track, IReadOnlyList<IRacingLeague.Models.Registration> members, CancellationToken cancellationToken)
    {
        for (var round = 0; round < 2; round++)
        {
            var race = await races.CreateAsync(leagueId, track, "GT3", DateTime.UtcNow.AddMonths(-2).AddDays(round * 14), 20, 20m, cancellationToken);
            var classification = members.Select((member, index) => new ClassificationEntryDTO
            {
                RegistrationId = member.RegistrationId,
                Position = round == 0 ? index + 1 : members.Count - index,
                FastestLapSeconds = 95m + index + round / 10m,
                IncidentPoints = index % 4,
                Dnf = round == 1 && index == 0,
                Notes = string.Empty
            }).OrderBy(x => x.Position).ToArray();
            await results.ReplaceClassificationAsync(race.RaceId, classification, cancellationToken);
        }

        await races.CreateAsync(leagueId, $"{track} (upcoming)", "GT3", DateTime.UtcNow.AddDays(14), 20, 18m, cancellationToken);
    }
}
