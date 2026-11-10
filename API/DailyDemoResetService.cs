namespace IRacingLeague.API;

public sealed class DailyDemoResetService(IServiceScopeFactory scopes, IConfiguration configuration, ILogger<DailyDemoResetService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("Demo:Enabled")) return;
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
                await seeder.SeedIfNeededAsync(reset: true, stoppingToken);
                logger.LogInformation("Demo data reset completed.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Demo data reset failed.");
            }
        }
    }
}
