using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<RefreshToken>().HasKey(t => t.RefreshTokenId);
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.FamilyId);
        modelBuilder.Entity<RefreshToken>().HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<League>().HasKey(l => l.LeagueId);
        modelBuilder.Entity<League>().Property(l => l.Version).HasColumnName("xmin").IsRowVersion();
        modelBuilder.Entity<Registration>().HasKey(r => r.RegistrationId);
        modelBuilder.Entity<Registration>().HasIndex(r => new { r.LeagueId, r.UserId }).IsUnique();
        modelBuilder.Entity<Registration>().HasIndex(r => new { r.LeagueId, r.CarNumber }).IsUnique();
        modelBuilder.Entity<Race>().HasKey(r => r.RaceId);
        modelBuilder.Entity<Race>().Property(r => r.Version).HasColumnName("xmin").IsRowVersion();
        modelBuilder.Entity<Result>().HasKey(r => r.ResultId);
        modelBuilder.Entity<Result>().HasIndex(r => new { r.RaceId, r.RegistrationId }).IsUnique();
        modelBuilder.Entity<Result>().HasIndex(r => new { r.RaceId, r.Position }).IsUnique();

        // --- Constraints ---
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Set decimal precision explicitly to keep the schema stable across providers.
        modelBuilder.Entity<League>().Property(l => l.EntryFee).HasPrecision(10, 2);
        modelBuilder.Entity<League>().Property(l => l.PointsTable).HasColumnType("integer[]").HasDefaultValue(new[] { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 });
        modelBuilder.Entity<League>().Property(l => l.FastestLapBonus).HasDefaultValue(1);
        modelBuilder.Entity<User>().Property(u => u.SafetyRating).HasPrecision(4, 2);
        modelBuilder.Entity<Registration>().Property(r => r.BallastKg).HasPrecision(6, 2);
        modelBuilder.Entity<Race>().Property(r => r.AmbientTempC).HasPrecision(5, 2);
        modelBuilder.Entity<Result>().Property(r => r.FastestLapSeconds).HasPrecision(8, 3);

        // The FK scalar already exists on each child (e.g. Registration.UserId), so we map to it
        // explicitly and rely on it rather than EF inventing a shadow FK.
        //
        // DeleteBehavior.Restrict on every relationship: Result has two parents (Registration, Race)
        // and Registration has two (User, League). Deletes here are administrative and rare, so Restrict is the safe choice —
        // a parent must be cleared of children before it can be removed.

        // User 1—* Registration
        modelBuilder.Entity<Registration>()
            .HasOne<User>().WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // League 1—* Registration
        modelBuilder.Entity<Registration>()
            .HasOne<League>().WithMany()
            .HasForeignKey(r => r.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        // League 1—* Race
        modelBuilder.Entity<Race>()
            .HasOne<League>().WithMany()
            .HasForeignKey(r => r.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Registration 1—* Result
        modelBuilder.Entity<Result>()
            .HasOne<Registration>().WithMany()
            .HasForeignKey(r => r.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Race 1—* Result
        modelBuilder.Entity<Result>()
            .HasOne<Race>().WithMany()
            .HasForeignKey(r => r.RaceId)
            .OnDelete(DeleteBehavior.Restrict);

        // League.OwnerUserId —* User (the owner; not a collection navigation on User)
        modelBuilder.Entity<League>()
            .HasOne<User>().WithMany()
            .HasForeignKey(l => l.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
