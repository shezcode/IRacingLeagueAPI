using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IRacingLeague.Data.EF;

/// <summary>Creates the context for migration tooling without loading runtime secrets.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=iracing_league_design;Username=design;Password=design")
            .Options;
        return new AppDbContext(options);
    }
}
