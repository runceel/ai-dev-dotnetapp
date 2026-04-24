using EventRegistration.Events.Application;
using EventRegistration.Events.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Events.Application;

[TestClass]
public sealed class EventsAppServiceTests
{
    private static ServiceProvider BuildProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<EventsDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventRepository, EventRepositoryForTest>();
        services.AddEventsApplication();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// テスト用の IEventRepository 実装。DI から EventsDbContext を受け取る。
    /// </summary>
    private sealed class EventRepositoryForTest(EventsDbContext db) : IEventRepository
    {
        public async Task<IReadOnlyList<EventRegistration.Events.Domain.Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await db.Events
                .OrderByDescending(e => e.ScheduledAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<EventRegistration.Events.Domain.Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task AddAsync(EventRegistration.Events.Domain.Event @event, CancellationToken cancellationToken = default)
        {
            await db.Events.AddAsync(@event, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsCreatedEvent()
    {
        var dbName = $"AppService_{nameof(CreateAsync_ReturnsCreatedEvent)}_{Guid.NewGuid()}";
        using var provider = BuildProvider(dbName);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();

        var input = new CreateEventInput
        {
            Name = "テストイベント",
            Description = "テスト説明",
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(7),
            Capacity = 50,
        };

        var result = await service.CreateAsync(input);

        Assert.AreNotEqual(Guid.Empty, result.Id);
        Assert.AreEqual("テストイベント", result.Name);
        Assert.AreEqual("テスト説明", result.Description);
        Assert.AreEqual(50, result.Capacity);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllEventsInDescendingOrder()
    {
        var dbName = $"AppService_{nameof(GetAllAsync_ReturnsAllEventsInDescendingOrder)}_{Guid.NewGuid()}";
        using var provider = BuildProvider(dbName);

        // Create events
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();
            await service.CreateAsync(new CreateEventInput
            {
                Name = "過去イベント",
                ScheduledAt = DateTimeOffset.UtcNow.AddDays(-1),
                Capacity = 10,
            });
            await service.CreateAsync(new CreateEventInput
            {
                Name = "未来イベント",
                ScheduledAt = DateTimeOffset.UtcNow.AddDays(10),
                Capacity = 20,
            });
        }

        // Read events
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();
            var results = await service.GetAllAsync();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("未来イベント", results[0].Name);
            Assert.AreEqual("過去イベント", results[1].Name);
        }
    }

    [TestMethod]
    public async Task GetByIdAsync_WithExistingId_ReturnsEvent()
    {
        var dbName = $"AppService_{nameof(GetByIdAsync_WithExistingId_ReturnsEvent)}_{Guid.NewGuid()}";
        using var provider = BuildProvider(dbName);

        Guid createdId;
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();
            var created = await service.CreateAsync(new CreateEventInput
            {
                Name = "テストイベント",
                ScheduledAt = DateTimeOffset.UtcNow.AddDays(7),
                Capacity = 30,
            });
            createdId = created.Id;
        }

        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();
            var result = await service.GetByIdAsync(createdId);

            Assert.IsNotNull(result);
            Assert.AreEqual(createdId, result.Id);
            Assert.AreEqual("テストイベント", result.Name);
        }
    }

    [TestMethod]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var dbName = $"AppService_{nameof(GetByIdAsync_WithNonExistentId_ReturnsNull)}_{Guid.NewGuid()}";
        using var provider = BuildProvider(dbName);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventsAppService>();

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.IsNull(result);
    }
}
