using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure;
using EventRegistration.Registrations.Infrastructure.Persistence;
using EventRegistration.Web.Adapters;
using EventRegistration.Web.DemoData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventRegistration.Registrations.Application.Services;

namespace EventRegistration.Web.Tests.DemoData;

[TestClass]
public sealed class RegistrationsDemoDataSeederTests
{
    private static IServiceProvider BuildProvider()
    {
        var dbId = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventsModuleInfrastructure();
        services.AddRegistrationsModuleInfrastructure();
        services.AddDbContext<EventsDbContext>(o =>
            o.UseInMemoryDatabase($"Events-{dbId}"), ServiceLifetime.Scoped);
        services.AddDbContext<RegistrationsDbContext>(o =>
            o.UseInMemoryDatabase($"Registrations-{dbId}"), ServiceLifetime.Scoped);
        services.AddScoped<IEventCapacityChecker, EventCapacityCheckerAdapter>();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task SeedAsync_DoesNothing_WhenNoEventsExist()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var register = scope.ServiceProvider.GetRequiredService<RegisterParticipantUseCase>();
        var registrationsDb = scope.ServiceProvider.GetRequiredService<RegistrationsDbContext>();

        var seeder = new RegistrationsDemoDataSeeder(eventRepo, register);
        await seeder.SeedAsync(CancellationToken.None);

        var count = await registrationsDb.Registrations.CountAsync();
        Assert.AreEqual(0, count, "Should not seed registrations when no events exist.");
    }

    [TestMethod]
    public async Task SeedAsync_SeedsRegistrations_WhenEventsExist()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var register = scope.ServiceProvider.GetRequiredService<RegisterParticipantUseCase>();
        var registrationsDb = scope.ServiceProvider.GetRequiredService<RegistrationsDbContext>();

        // Pre-seed an event with capacity 2.
        await eventRepo.AddAsync(Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 2));

        var seeder = new RegistrationsDemoDataSeeder(eventRepo, register);
        await seeder.SeedAsync(CancellationToken.None);

        var registrations = await registrationsDb.Registrations.ToListAsync();
        Assert.IsTrue(registrations.Count >= 2, $"Expected at least 2 registrations, got {registrations.Count}.");
        // capacity=2 → at least one should be Confirmed and one WaitListed (we seed capacity+1).
        Assert.IsTrue(registrations.Any(r => r.Status == RegistrationStatus.Confirmed));
    }

    [TestMethod]
    public async Task SeedAsync_IsIdempotent()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var register = scope.ServiceProvider.GetRequiredService<RegisterParticipantUseCase>();
        var registrationsDb = scope.ServiceProvider.GetRequiredService<RegistrationsDbContext>();

        await eventRepo.AddAsync(Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 2));

        var seeder = new RegistrationsDemoDataSeeder(eventRepo, register);
        await seeder.SeedAsync(CancellationToken.None);
        var firstRunCount = await registrationsDb.Registrations.CountAsync();

        await seeder.SeedAsync(CancellationToken.None);
        var secondRunCount = await registrationsDb.Registrations.CountAsync();

        Assert.AreEqual(firstRunCount, secondRunCount, "Re-running seeder should not add more registrations.");
    }
}
