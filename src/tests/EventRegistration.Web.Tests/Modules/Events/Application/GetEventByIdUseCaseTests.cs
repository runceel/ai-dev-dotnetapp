using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Application;

[TestClass]
public sealed class GetEventByIdUseCaseTests
{
    private static readonly DateTimeOffset ScheduledAt = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 25, 0, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public async Task ExecuteAsync_WithExistingId_ReturnsEvent()
    {
        var repository = new InMemoryEventRepository();
        var @event = Event.Create("テストイベント", "説明", ScheduledAt, 50, CreatedAt);
        await repository.AddAsync(@event);

        var useCase = new GetEventByIdUseCase(repository);
        var result = await useCase.ExecuteAsync(@event.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(@event.Id, result.Id);
        Assert.AreEqual("テストイベント", result.Name);
    }

    [TestMethod]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        var repository = new InMemoryEventRepository();
        var useCase = new GetEventByIdUseCase(repository);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        Assert.IsNull(result);
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
