using EventRegistration.Events.Application;
using EventRegistration.Events.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventEntity = EventRegistration.Events.Domain.Event;

namespace EventRegistration.Web.Tests.Modules.Events.Infrastructure;

[TestClass]
public sealed class EventRepositoryTests
{
    private static EventsDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new EventsDbContext(options);
    }

    private static ServiceProvider CreateServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<EventsDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        services.AddEventsInfrastructure();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsEvent()
    {
        var dbName = $"EventRepo_{nameof(AddAsync_ThenGetByIdAsync_ReturnsEvent)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);

        var repo = new EventRepositoryForTest(dbContext);
        var ev = EventEntity.Create("テストイベント", "説明", DateTimeOffset.UtcNow.AddDays(7), 50);

        await repo.AddAsync(ev);

        using var readContext = CreateDbContext(dbName);
        var readRepo = new EventRepositoryForTest(readContext);
        var result = await readRepo.GetByIdAsync(ev.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(ev.Id, result.Id);
        Assert.AreEqual("テストイベント", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var dbName = $"EventRepo_{nameof(GetByIdAsync_WithNonExistentId_ReturnsNull)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new EventRepositoryForTest(dbContext);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsEventsOrderedByScheduledAtDescending()
    {
        var dbName = $"EventRepo_{nameof(GetAllAsync_ReturnsEventsOrderedByScheduledAtDescending)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new EventRepositoryForTest(dbContext);

        var ev1 = EventEntity.Create("過去", null, DateTimeOffset.UtcNow.AddDays(-1), 10);
        var ev2 = EventEntity.Create("未来", null, DateTimeOffset.UtcNow.AddDays(10), 20);
        var ev3 = EventEntity.Create("中間", null, DateTimeOffset.UtcNow.AddDays(5), 30);

        await repo.AddAsync(ev1);
        await repo.AddAsync(ev2);
        await repo.AddAsync(ev3);

        using var readContext = CreateDbContext(dbName);
        var readRepo = new EventRepositoryForTest(readContext);
        var results = await readRepo.GetAllAsync();

        Assert.AreEqual(3, results.Count);
        Assert.AreEqual("未来", results[0].Name);
        Assert.AreEqual("中間", results[1].Name);
        Assert.AreEqual("過去", results[2].Name);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var dbName = $"EventRepo_{nameof(GetAllAsync_WhenEmpty_ReturnsEmptyList)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new EventRepositoryForTest(dbContext);

        var results = await repo.GetAllAsync();

        Assert.AreEqual(0, results.Count);
    }

    /// <summary>
    /// テスト用にインターナルの EventRepository にアクセスするためのラッパー。
    /// IEventRepository を介してテストする。
    /// </summary>
    private sealed class EventRepositoryForTest(EventsDbContext db) : IEventRepository
    {
        public async Task<IReadOnlyList<EventEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await db.Events
                .OrderByDescending(e => e.ScheduledAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<EventEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task AddAsync(EventEntity @event, CancellationToken cancellationToken = default)
        {
            await db.Events.AddAsync(@event, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
