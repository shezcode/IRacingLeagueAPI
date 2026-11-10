using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfUserRepository(AppDbContext context) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Add(user); return Task.CompletedTask; }
    public Task<User?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Users.FindAsync([id], cancellationToken).AsTask();
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) => context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public async Task<PagedResponse<User>> QueryAsync(UserQueryParameters p, CancellationToken cancellationToken = default)
    {
        var q = context.Users.AsNoTracking().AsQueryable();
        if (p.MinIRating.HasValue) q = q.Where(u => u.IRating >= p.MinIRating.Value);
        if (!string.IsNullOrWhiteSpace(p.LicenseClass)) q = q.Where(u => u.LicenseClass == p.LicenseClass);
        q = p.SortBy?.ToLowerInvariant() switch
        {
            "totalwins" => p.Desc ? q.OrderByDescending(u => u.TotalWins) : q.OrderBy(u => u.TotalWins),
            "username" => p.Desc ? q.OrderByDescending(u => u.UserName) : q.OrderBy(u => u.UserName),
            _ => p.Desc ? q.OrderByDescending(u => u.IRating) : q.OrderBy(u => u.IRating)
        };
        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<User>(items, p.Page, p.PageSize, totalCount);
    }
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Update(user); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var user = await context.Users.FindAsync([id], cancellationToken); if (user is not null) context.Users.Remove(user); }
}
