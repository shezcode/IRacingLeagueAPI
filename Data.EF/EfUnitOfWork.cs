using IRacingLeague.Data;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        // Service operations can compose (for example, deleting a league calls the
        // registration and race deletion services). Reuse the surrounding transaction
        // instead of trying to begin a nested PostgreSQL transaction.
        if (context.Database.CurrentTransaction is not null)
        {
            await operation(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
