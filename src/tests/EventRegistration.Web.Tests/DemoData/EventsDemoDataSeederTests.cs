using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Web.DemoData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.DemoData;

[TestClass]
public sealed class EventsDemoDataSeederTests
{
    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddEventsModuleInfrastructure();
        // Use a unique DB name to avoid cross-test interference.
        services.AddDbContext<EventsDbContext>(o =>
            o.UseInMemoryDatabase($"Events-{Guid.NewGuid()}"), ServiceLifetime.Scoped);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task SeedAsync_PopulatesEvents_WhenDbIsEmpty()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var seeder = new EventsDemoDataSeeder(repo);

        await seeder.SeedAsync(CancellationToken.None);

        var events = await repo.GetAllAsync();
        Assert.IsTrue(events.Count >= 3, $"Expected at least 3 seeded events, got {events.Count}.");
    }

    [TestMethod]
    public async Task SeedAsync_IsIdempotent_WhenAlreadySeeded()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        // Pre-seed with one event so the seeder should not run.
        await repo.AddAsync(Event.Create("既存イベント", null, DateTimeOffset.UtcNow.AddDays(1), 5));
        var beforeCount = (await repo.GetAllAsync()).Count;

        var seeder = new EventsDemoDataSeeder(repo);
        await seeder.SeedAsync(CancellationToken.None);
        await seeder.SeedAsync(CancellationToken.None);

        var afterCount = (await repo.GetAllAsync()).Count;
        Assert.AreEqual(beforeCount, afterCount, "Seeder must be a no-op when events already exist.");
    }
}
