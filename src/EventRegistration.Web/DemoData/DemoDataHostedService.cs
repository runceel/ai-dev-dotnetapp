using EventRegistration.SharedKernel.Application.DemoData;
using Microsoft.Extensions.Options;

namespace EventRegistration.Web.DemoData;

/// <summary>
/// アプリケーション起動時に <see cref="IDemoDataSeeder"/> 群を実行する <see cref="IHostedService"/>。
/// <see cref="DemoDataOptions.Enabled"/> が <c>false</c> の場合は何もしない。
/// </summary>
public sealed class DemoDataHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<DemoDataOptions> options,
    ILogger<DemoDataHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("DemoData seeding is disabled (DemoData:Enabled=false). Skipping.");
            return;
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var seeders = scope.ServiceProvider
            .GetServices<IDemoDataSeeder>()
            .OrderBy(s => s.Order)
            .ToList();

        if (seeders.Count == 0)
        {
            logger.LogInformation("DemoData seeding is enabled but no IDemoDataSeeder is registered.");
            return;
        }

        logger.LogInformation("DemoData seeding started ({Count} seeder(s)).", seeders.Count);

        foreach (var seeder in seeders)
        {
            try
            {
                await seeder.SeedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // 起動を妨げないようログ出力のみ行う。
                logger.LogWarning(ex,
                    "DemoData seeder {SeederType} failed; continuing application startup.",
                    seeder.GetType().Name);
            }
        }

        logger.LogInformation("DemoData seeding completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
