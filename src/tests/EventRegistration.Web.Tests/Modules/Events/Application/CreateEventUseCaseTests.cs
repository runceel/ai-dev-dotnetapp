using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Application;

[TestClass]
public sealed class CreateEventUseCaseTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 4, 25, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ValidScheduledAt = new(2026, 6, 1, 10, 0, 0, TimeSpan.FromHours(9));

    [TestMethod]
    public async Task ExecuteAsync_WithValidInput_ReturnsNewEventId()
    {
        var repository = new InMemoryEventRepository();
        var timeProvider = new FakeTimeProvider(FixedNow);
        var useCase = new CreateEventUseCase(repository, timeProvider);

        var id = await useCase.ExecuteAsync("テストイベント", "説明", ValidScheduledAt, 100);

        Assert.AreNotEqual(Guid.Empty, id);
    }

    [TestMethod]
    public async Task ExecuteAsync_PersistsEvent()
    {
        var repository = new InMemoryEventRepository();
        var timeProvider = new FakeTimeProvider(FixedNow);
        var useCase = new CreateEventUseCase(repository, timeProvider);

        var id = await useCase.ExecuteAsync("テストイベント", "説明", ValidScheduledAt, 100);
        var saved = await repository.GetByIdAsync(id);

        Assert.IsNotNull(saved);
        Assert.AreEqual("テストイベント", saved.Name);
        Assert.AreEqual("説明", saved.Description);
        Assert.AreEqual(ValidScheduledAt, saved.ScheduledAt);
        Assert.AreEqual(100, saved.Capacity);
        Assert.AreEqual(FixedNow, saved.CreatedAt);
    }

    [TestMethod]
    public async Task ExecuteAsync_SetsCreatedAtFromTimeProvider()
    {
        var customNow = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryEventRepository();
        var timeProvider = new FakeTimeProvider(customNow);
        var useCase = new CreateEventUseCase(repository, timeProvider);

        var id = await useCase.ExecuteAsync("イベント", null, ValidScheduledAt, 10);
        var saved = await repository.GetByIdAsync(id);

        Assert.IsNotNull(saved);
        Assert.AreEqual(customNow, saved.CreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ExecuteAsync_WithEmptyName_ThrowsArgumentException()
    {
        var repository = new InMemoryEventRepository();
        var timeProvider = new FakeTimeProvider(FixedNow);
        var useCase = new CreateEventUseCase(repository, timeProvider);

        await useCase.ExecuteAsync("", null, ValidScheduledAt, 10);
    }
}

/// <summary>
/// テスト用のインメモリ IEventRepository 実装。
/// </summary>
file sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events.FirstOrDefault(e => e.Id == id));
    }

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Event>>(_events.AsReadOnly());
    }

    public Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _events.Add(@event);
        return Task.CompletedTask;
    }
}

/// <summary>
/// テスト用の固定時刻を返す TimeProvider。
/// </summary>
file sealed class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
