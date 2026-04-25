using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Web.Adapters;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Web.Tests.Adapters;

[TestClass]
public sealed class EventCapacityCheckerAdapterTests
{
    [TestMethod]
    public async Task GetEventCapacityInfoAsync_EventExists_ReturnsCapacityInfo()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase($"Events-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new EventsDbContext(options);
        var ev = Event.Create("テストイベント", "説明", DateTimeOffset.UtcNow.AddDays(7), 25);
        dbContext.Events.Add(ev);
        await dbContext.SaveChangesAsync();

        var adapter = new EventCapacityCheckerAdapter(dbContext);

        var result = await adapter.GetEventCapacityInfoAsync(ev.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(ev.Id, result.EventId);
        Assert.AreEqual("テストイベント", result.EventName);
        Assert.AreEqual(25, result.Capacity);
    }

    [TestMethod]
    public async Task GetEventCapacityInfoAsync_EventNotExists_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase($"Events-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new EventsDbContext(options);
        var adapter = new EventCapacityCheckerAdapter(dbContext);

        var result = await adapter.GetEventCapacityInfoAsync(Guid.NewGuid());

        Assert.IsNull(result);
    }
}
