using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Web.Tests.Modules.Events.Infrastructure;

[TestClass]
public sealed class EventRepositoryTests
{
    private static readonly DateTimeOffset ScheduledAt = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 25, 0, 0, 0, TimeSpan.Zero);

    private static EventsDbContext CreateDbContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new EventsDbContext(options);
    }

    [TestMethod]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsPersistedEvent()
    {
        var dbName = Guid.NewGuid().ToString();
        var @event = Event.Create("テストイベント", "説明", ScheduledAt, 100, CreatedAt);

        // 書き込み用コンテキスト
        using (var writeContext = CreateDbContext(dbName))
        {
            var repository = new EventRepository(writeContext);
            await repository.AddAsync(@event);
        }

        // 読み取り用コンテキスト（別インスタンスで永続化を確認）
        using (var readContext = CreateDbContext(dbName))
        {
            var repository = new EventRepository(readContext);
            var result = await repository.GetByIdAsync(@event.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(@event.Id, result.Id);
            Assert.AreEqual("テストイベント", result.Name);
            Assert.AreEqual("説明", result.Description);
            Assert.AreEqual(ScheduledAt, result.ScheduledAt);
            Assert.AreEqual(100, result.Capacity);
            Assert.AreEqual(CreatedAt, result.CreatedAt);
        }
    }

    [TestMethod]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        using var context = CreateDbContext();
        var repository = new EventRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllAsync_WithNoEvents_ReturnsEmptyList()
    {
        using var context = CreateDbContext();
        var repository = new EventRepository(context);

        var result = await repository.GetAllAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllPersistedEvents()
    {
        var dbName = Guid.NewGuid().ToString();
        var event1 = Event.Create("イベント1", null, ScheduledAt, 10, CreatedAt);
        var event2 = Event.Create("イベント2", null, ScheduledAt, 20, CreatedAt);

        using (var writeContext = CreateDbContext(dbName))
        {
            var repository = new EventRepository(writeContext);
            await repository.AddAsync(event1);
            await repository.AddAsync(event2);
        }

        using (var readContext = CreateDbContext(dbName))
        {
            var repository = new EventRepository(readContext);
            var result = await repository.GetAllAsync();

            Assert.AreEqual(2, result.Count);
        }
    }

    [TestMethod]
    public async Task AddAsync_SavesChangesImmediately()
    {
        var dbName = Guid.NewGuid().ToString();
        var @event = Event.Create("イベント", null, ScheduledAt, 10, CreatedAt);

        using (var context = CreateDbContext(dbName))
        {
            var repository = new EventRepository(context);
            await repository.AddAsync(@event);
        }

        // 別コンテキストで読めることを確認（SaveChanges 済み）
        using (var context = CreateDbContext(dbName))
        {
            var count = await context.Events.CountAsync();
            Assert.AreEqual(1, count);
        }
    }
}
