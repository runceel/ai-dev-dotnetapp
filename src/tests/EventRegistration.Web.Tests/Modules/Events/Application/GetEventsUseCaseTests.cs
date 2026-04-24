using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Application;

[TestClass]
public sealed class GetEventsUseCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 25, 0, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public async Task ExecuteAsync_WithNoEvents_ReturnsEmptyList()
    {
        var repository = new InMemoryEventRepository();
        var useCase = new GetEventsUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ExecuteAsync_ReturnsEventsInDescendingScheduledAtOrder()
    {
        var repository = new InMemoryEventRepository();
        var earlier = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var later = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);
        var latest = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);

        await repository.AddAsync(Event.Create("イベント1", null, earlier, 10, CreatedAt));
        await repository.AddAsync(Event.Create("イベント3", null, latest, 10, CreatedAt));
        await repository.AddAsync(Event.Create("イベント2", null, later, 10, CreatedAt));

        var useCase = new GetEventsUseCase(repository);
        var result = await useCase.ExecuteAsync();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("イベント3", result[0].Name);
        Assert.AreEqual("イベント2", result[1].Name);
        Assert.AreEqual("イベント1", result[2].Name);
    }

    [TestMethod]
    public async Task ExecuteAsync_ReturnsAllEvents()
    {
        var repository = new InMemoryEventRepository();
        var scheduledAt = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);

        await repository.AddAsync(Event.Create("A", null, scheduledAt, 10, CreatedAt));
        await repository.AddAsync(Event.Create("B", null, scheduledAt, 20, CreatedAt));

        var useCase = new GetEventsUseCase(repository);
        var result = await useCase.ExecuteAsync();

        Assert.AreEqual(2, result.Count);
    }
}

file sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_events.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Event>>(_events.AsReadOnly());

    public Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _events.Add(@event);
        return Task.CompletedTask;
    }
}
