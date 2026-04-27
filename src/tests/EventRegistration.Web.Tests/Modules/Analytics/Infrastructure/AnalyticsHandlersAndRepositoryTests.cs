using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.Analytics.Infrastructure;
using EventRegistration.Analytics.Infrastructure.Handlers;
using EventRegistration.Analytics.Infrastructure.Persistence;
using EventRegistration.SharedKernel.Application.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventRegistration.Web.Tests.Modules.Analytics.Infrastructure;

[TestClass]
public sealed class AnalyticsHandlersAndRepositoryTests
{
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;
    private AnalyticsDbContext _db = null!;
    private IRegistrationActivityRepository _repo = null!;
    private Guid _eventId;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AnalyticsDbContext>(o =>
            o.UseInMemoryDatabase($"Analytics-{Guid.NewGuid()}"));
        services.AddScoped<IRegistrationActivityRepository, RegistrationActivityRepository>();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        _repo = _scope.ServiceProvider.GetRequiredService<IRegistrationActivityRepository>();
        _eventId = Guid.NewGuid();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [TestMethod]
    public async Task ConfirmedHandler_PersistsConfirmedActivity()
    {
        var handler = new ParticipantConfirmedAnalyticsHandler(_repo);
        var ev = new ParticipantConfirmedEvent(
            Guid.NewGuid(), _eventId, "太郎", "taro@example.com", DateTimeOffset.UtcNow);

        await handler.HandleAsync(ev);

        var activities = await _db.Activities.ToListAsync();
        Assert.AreEqual(1, activities.Count);
        Assert.AreEqual(RegistrationActivityType.Confirmed, activities[0].ActivityType);
        Assert.AreEqual(_eventId, activities[0].EventId);
    }

    [TestMethod]
    public async Task WaitListedHandler_PersistsWaitListedActivity()
    {
        var handler = new ParticipantWaitListedAnalyticsHandler(_repo);
        var ev = new ParticipantWaitListedEvent(
            Guid.NewGuid(), _eventId, "花子", "hanako@example.com", DateTimeOffset.UtcNow);

        await handler.HandleAsync(ev);

        var activities = await _db.Activities.ToListAsync();
        Assert.AreEqual(1, activities.Count);
        Assert.AreEqual(RegistrationActivityType.WaitListed, activities[0].ActivityType);
    }

    [TestMethod]
    public async Task CancelledHandler_PersistsCancelledActivity()
    {
        var handler = new RegistrationCancelledAnalyticsHandler(_repo);
        var ev = new RegistrationCancelledEvent(
            Guid.NewGuid(), _eventId, RegistrationCancelledPriorStatus.Confirmed, DateTimeOffset.UtcNow);

        await handler.HandleAsync(ev);

        var activities = await _db.Activities.ToListAsync();
        Assert.AreEqual(1, activities.Count);
        Assert.AreEqual(RegistrationActivityType.Cancelled, activities[0].ActivityType);
    }

    [TestMethod]
    public async Task PromotedHandler_PersistsPromotedActivity()
    {
        var handler = new ParticipantPromotedFromWaitListAnalyticsHandler(_repo);
        var ev = new ParticipantPromotedFromWaitListEvent(
            Guid.NewGuid(), _eventId, "次郎", "jiro@example.com", DateTimeOffset.UtcNow);

        await handler.HandleAsync(ev);

        var activities = await _db.Activities.ToListAsync();
        Assert.AreEqual(1, activities.Count);
        Assert.AreEqual(RegistrationActivityType.PromotedFromWaitList, activities[0].ActivityType);
    }

    [TestMethod]
    public async Task GetEventStatisticsAsync_AggregatesByActivityType()
    {
        await SeedAsync(RegistrationActivityType.Confirmed, 3);
        await SeedAsync(RegistrationActivityType.WaitListed, 2);
        await SeedAsync(RegistrationActivityType.Cancelled, 1);
        await SeedAsync(RegistrationActivityType.PromotedFromWaitList, 1);
        // 別イベントに 1 件追加（フィルタリング検証）
        var otherEventId = Guid.NewGuid();
        await _repo.AddAsync(RegistrationActivity.Create(
            otherEventId, Guid.NewGuid(),
            RegistrationActivityType.Confirmed, DateTimeOffset.UtcNow));
        await _repo.SaveChangesAsync();

        var stats = await _repo.GetEventStatisticsAsync(_eventId);

        Assert.AreEqual(_eventId, stats.EventId);
        Assert.AreEqual(3, stats.ConfirmedCount);
        Assert.AreEqual(2, stats.WaitListedCount);
        Assert.AreEqual(1, stats.CancelledCount);
        Assert.AreEqual(1, stats.PromotedCount);
        Assert.AreEqual(5, stats.TotalRegistrations);
        Assert.AreEqual(3, stats.FinalConfirmedCount); // 3 + 1 - 1 = 3
    }

    [TestMethod]
    public async Task GetEventStatisticsAsync_NoActivities_ReturnsZeros()
    {
        var stats = await _repo.GetEventStatisticsAsync(Guid.NewGuid());

        Assert.AreEqual(0, stats.ConfirmedCount);
        Assert.AreEqual(0, stats.WaitListedCount);
        Assert.AreEqual(0, stats.CancelledCount);
        Assert.AreEqual(0, stats.PromotedCount);
    }

    [TestMethod]
    public async Task GetDailyStatisticsAsync_GroupsByUtcDate_AndIncludesEmptyDays()
    {
        var day1 = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var day3 = new DateTime(2026, 1, 12, 23, 30, 0, DateTimeKind.Utc);

        await _repo.AddAsync(RegistrationActivity.Create(
            _eventId, Guid.NewGuid(), RegistrationActivityType.Confirmed, day1));
        await _repo.AddAsync(RegistrationActivity.Create(
            _eventId, Guid.NewGuid(), RegistrationActivityType.Confirmed, day1.AddHours(1)));
        await _repo.AddAsync(RegistrationActivity.Create(
            _eventId, Guid.NewGuid(), RegistrationActivityType.Cancelled, day3));
        await _repo.SaveChangesAsync();

        var result = await _repo.GetDailyStatisticsAsync(
            _eventId,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, result[0].ConfirmedCount);
        Assert.AreEqual(0, result[1].ConfirmedCount);
        Assert.AreEqual(0, result[1].CancelledCount);
        Assert.AreEqual(1, result[2].CancelledCount);
    }

    [TestMethod]
    public async Task GetTrackedEventIdsAsync_ReturnsDistinctEventIds()
    {
        var other = Guid.NewGuid();
        await _repo.AddAsync(RegistrationActivity.Create(
            _eventId, Guid.NewGuid(), RegistrationActivityType.Confirmed, DateTimeOffset.UtcNow));
        await _repo.AddAsync(RegistrationActivity.Create(
            _eventId, Guid.NewGuid(), RegistrationActivityType.Cancelled, DateTimeOffset.UtcNow));
        await _repo.AddAsync(RegistrationActivity.Create(
            other, Guid.NewGuid(), RegistrationActivityType.Confirmed, DateTimeOffset.UtcNow));
        await _repo.SaveChangesAsync();

        var ids = await _repo.GetTrackedEventIdsAsync();

        Assert.AreEqual(2, ids.Count);
        CollectionAssert.AreEquivalent(new[] { _eventId, other }, ids.ToArray());
    }

    private async Task SeedAsync(RegistrationActivityType type, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await _repo.AddAsync(RegistrationActivity.Create(
                _eventId, Guid.NewGuid(), type, DateTimeOffset.UtcNow));
        }
        await _repo.SaveChangesAsync();
    }
}

[TestClass]
public sealed class AnalyticsModuleInfrastructureExtensionsTests
{
    [TestMethod]
    public void AddAnalyticsModule_RegistersAllExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddLogging();
        services.AddAnalyticsModule();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsNotNull(scope.ServiceProvider.GetService<AnalyticsDbContext>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IRegistrationActivityRepository>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<EventRegistration.Analytics.Application.UseCases.GetEventStatisticsUseCase>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<EventRegistration.Analytics.Application.UseCases.GetDailyStatisticsUseCase>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IDomainEventDispatcher>());

        Assert.IsNotNull(scope.ServiceProvider.GetService<IDomainEventHandler<ParticipantConfirmedEvent>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IDomainEventHandler<ParticipantWaitListedEvent>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IDomainEventHandler<RegistrationCancelledEvent>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IDomainEventHandler<ParticipantPromotedFromWaitListEvent>>());
    }
}
