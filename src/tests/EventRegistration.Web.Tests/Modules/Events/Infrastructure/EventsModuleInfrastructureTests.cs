using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Events.Infrastructure;

[TestClass]
public sealed class EventsModuleInfrastructureTests
{
    [TestMethod]
    public void AddEventsModuleInfrastructure_RegistersDbContext()
    {
        var services = new ServiceCollection();

        services.AddEventsModuleInfrastructure();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(EventsDbContext));
        Assert.IsNotNull(descriptor);
    }

    [TestMethod]
    public void AddEventsModuleInfrastructure_RegistersRepository()
    {
        var services = new ServiceCollection();

        services.AddEventsModuleInfrastructure();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEventRepository));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddEventsModuleInfrastructure_RegistersUseCases()
    {
        var services = new ServiceCollection();

        services.AddEventsModuleInfrastructure();

        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(CreateEventUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetEventByIdUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetAllEventsUseCase)));
    }

    [TestMethod]
    public void AddEventsModuleInfrastructure_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddEventsModuleInfrastructure();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public async Task EventRepository_AddAndRetrieve_Works()
    {
        var services = new ServiceCollection();
        services.AddEventsModuleInfrastructure();
        // Use a unique DB name to avoid cross-test interference
        services.AddDbContext<EventsDbContext>(o =>
            o.UseInMemoryDatabase($"Events-{Guid.NewGuid()}"), ServiceLifetime.Scoped);
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var ev = EventRegistration.Events.Domain.Event.Create("テスト", "説明", DateTimeOffset.UtcNow, 10);
        await repo.AddAsync(ev);

        var retrieved = await repo.GetByIdAsync(ev.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("テスト", retrieved.Name);
    }
}
