using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Infrastructure;
using EventRegistration.Analytics.Infrastructure.Persistence;
using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Web.DemoData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.DemoData;

[TestClass]
public sealed class AnalyticsDemoDataSeederTests
{
    private static IServiceProvider BuildProvider()
    {
        var dbId = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventsModuleInfrastructure();
        services.AddAnalyticsModule();
        services.AddDbContext<EventsDbContext>(o =>
            o.UseInMemoryDatabase($"Events-{dbId}"), ServiceLifetime.Scoped);
        services.AddDbContext<AnalyticsDbContext>(o =>
            o.UseInMemoryDatabase($"Analytics-{dbId}"), ServiceLifetime.Scoped);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task SeedAsync_DoesNothing_WhenNoEventsExist()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var activityRepo = scope.ServiceProvider.GetRequiredService<IRegistrationActivityRepository>();
        var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        var seeder = new AnalyticsDemoDataSeeder(eventRepo, activityRepo);
        await seeder.SeedAsync(CancellationToken.None);

        var count = await analyticsDb.Activities.CountAsync();
        Assert.AreEqual(0, count, "Should not seed analytics when no events exist.");
    }

    [TestMethod]
    public async Task SeedAsync_CreatesActivitiesForAllEvents()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var activityRepo = scope.ServiceProvider.GetRequiredService<IRegistrationActivityRepository>();
        var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        // Pre-seed 2 events
        await eventRepo.AddAsync(Event.Create("テスト1", null, DateTimeOffset.UtcNow.AddDays(7), 10));
        await eventRepo.AddAsync(Event.Create("テスト2", null, DateTimeOffset.UtcNow.AddDays(14), 20));

        var seeder = new AnalyticsDemoDataSeeder(eventRepo, activityRepo);
        await seeder.SeedAsync(CancellationToken.None);

        var activities = await analyticsDb.Activities.ToListAsync();
        Assert.IsTrue(activities.Count > 10, $"Expected significant activity count, got {activities.Count}.");

        // Verify activities span multiple event IDs
        var eventIds = activities.Select(a => a.EventId).Distinct().ToList();
        Assert.AreEqual(2, eventIds.Count, "Should have activities for both events.");

        // Verify all 4 activity types present
        var types = activities.Select(a => a.ActivityType).Distinct().ToList();
        Assert.IsTrue(types.Count == 4, $"Expected all 4 activity types, got {types.Count}.");
    }

    [TestMethod]
    public async Task SeedAsync_IsIdempotent()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var activityRepo = scope.ServiceProvider.GetRequiredService<IRegistrationActivityRepository>();
        var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        await eventRepo.AddAsync(Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 10));

        var seeder = new AnalyticsDemoDataSeeder(eventRepo, activityRepo);
        await seeder.SeedAsync(CancellationToken.None);
        var firstRunCount = await analyticsDb.Activities.CountAsync();

        await seeder.SeedAsync(CancellationToken.None);
        var secondRunCount = await analyticsDb.Activities.CountAsync();

        Assert.AreEqual(firstRunCount, secondRunCount, "Re-running seeder should not add more activities.");
    }

    [TestMethod]
    public async Task SeedAsync_ActivitiesSpanMultipleDays()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var activityRepo = scope.ServiceProvider.GetRequiredService<IRegistrationActivityRepository>();
        var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        await eventRepo.AddAsync(Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 20));

        var seeder = new AnalyticsDemoDataSeeder(eventRepo, activityRepo);
        await seeder.SeedAsync(CancellationToken.None);

        var activities = await analyticsDb.Activities.ToListAsync();
        var distinctDays = activities
            .Select(a => DateOnly.FromDateTime(a.OccurredAt.UtcDateTime))
            .Distinct()
            .Count();
        Assert.IsTrue(distinctDays >= 5, $"Activities should span at least 5 distinct days for a good chart, got {distinctDays}.");
    }
}
