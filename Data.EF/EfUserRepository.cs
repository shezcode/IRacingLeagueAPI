using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfUserRepository(AppDbContext context) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Add(user); return Task.CompletedTask; }
    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([id], cancellationToken);
        if (user is not null) user.RacesCompleted = await (from result in context.Results join registration in context.Registrations on result.RegistrationId equals registration.RegistrationId where registration.UserId == id select result.ResultId).CountAsync(cancellationToken);
        return user;
    }
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) => context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public async Task<PagedResponse<User>> QueryAsync(UserQueryParameters p, CancellationToken cancellationToken = default)
    {
        var q = context.Users.AsNoTracking().AsQueryable();
        if (p.MinIRating.HasValue) q = q.Where(u => u.IRating >= p.MinIRating.Value);
        if (!string.IsNullOrWhiteSpace(p.MinLicenseClass)) { var order = new[] { "Rookie", "D", "C", "B", "A" }; var min = Array.FindIndex(order, c => c.Equals(p.MinLicenseClass, StringComparison.OrdinalIgnoreCase)); if (min >= 0) q = q.Where(u => order.Skip(min).Contains(u.LicenseClass)); }
        if (!string.IsNullOrWhiteSpace(p.LicenseClass)) q = q.Where(u => u.LicenseClass == p.LicenseClass);
        q = p.SortBy?.ToLowerInvariant() switch
        {
            "wins" or "totalwins" => p.Desc ? q.OrderByDescending(u => u.TotalWins) : q.OrderBy(u => u.TotalWins),
            "username" => p.Desc ? q.OrderByDescending(u => u.UserName) : q.OrderBy(u => u.UserName),
            _ => p.Desc ? q.OrderByDescending(u => u.IRating) : q.OrderBy(u => u.IRating)
        };
        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        var ids = items.Select(u => u.UserId).ToArray();
        var counts = await (from result in context.Results join registration in context.Registrations on result.RegistrationId equals registration.RegistrationId where ids.Contains(registration.UserId) group result by registration.UserId into g select new { UserId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
        foreach (var user in items) user.RacesCompleted = counts.GetValueOrDefault(user.UserId);
        return new PagedResponse<User>(items, p.Page, p.PageSize, totalCount);
    }
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Update(user); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var user = await context.Users.FindAsync([id], cancellationToken); if (user is not null) context.Users.Remove(user); }
}
