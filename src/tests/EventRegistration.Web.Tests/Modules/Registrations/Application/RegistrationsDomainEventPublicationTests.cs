using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure.Persistence;
using EventRegistration.SharedKernel.Application.Events;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Web.Tests.Modules.Registrations.Application;

/// <summary>
/// Registrations の UseCase がドメインイベントを発行することを検証するテスト。
/// </summary>
[TestClass]
public sealed class RegistrationsDomainEventPublicationTests
{
    private RegistrationsDbContext _registrationsDb = null!;
    private EventsDbContext _eventsDb = null!;
    private RegisterParticipantUseCase _registerUseCase = null!;
    private CancelRegistrationUseCase _cancelUseCase = null!;
    private RecordingDomainEventDispatcher _dispatcher = null!;
    private Guid _eventId;

    [TestInitialize]
    public async Task Setup()
    {
        var dbId = Guid.NewGuid().ToString();

        _eventsDb = new EventsDbContext(
            new DbContextOptionsBuilder<EventsDbContext>()
                .UseInMemoryDatabase($"Events-{dbId}").Options);
        _registrationsDb = new RegistrationsDbContext(
            new DbContextOptionsBuilder<RegistrationsDbContext>()
                .UseInMemoryDatabase($"Registrations-{dbId}").Options);

        var ev = Event.Create("テストイベント", "テスト用", DateTimeOffset.UtcNow.AddDays(7), capacity: 2);
        _eventId = ev.Id;
        _eventsDb.Events.Add(ev);
        await _eventsDb.SaveChangesAsync();

        var repo = new RegistrationRepository(_registrationsDb);
        var capacityChecker = new TestEventCapacityChecker(_eventsDb);
        _dispatcher = new RecordingDomainEventDispatcher();

        _registerUseCase = new RegisterParticipantUseCase(repo, capacityChecker, _dispatcher);
        _cancelUseCase = new CancelRegistrationUseCase(repo, _dispatcher);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _registrationsDb.Dispose();
        _eventsDb.Dispose();
    }

    [TestMethod]
    public async Task Register_WhenConfirmed_PublishesParticipantConfirmedEvent()
    {
        var result = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, _dispatcher.Dispatched.Count);
        var ev = _dispatcher.Dispatched[0] as ParticipantConfirmedEvent;
        Assert.IsNotNull(ev);
        Assert.AreEqual(result.Registration.Id, ev!.RegistrationId);
        Assert.AreEqual(_eventId, ev.EventId);
        Assert.AreEqual("太郎", ev.ParticipantName);
        Assert.AreEqual("taro@example.com", ev.ParticipantEmail);
    }

    [TestMethod]
    public async Task Register_WhenWaitListed_DoesNotPublishEvent()
    {
        // 定員 2 を埋める（イベント 2 件発行される）
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");

        _dispatcher.Dispatched.Clear();

        var result = await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(RegistrationStatus.WaitListed, result.Registration.Status);
        Assert.AreEqual(0, _dispatcher.Dispatched.Count);
    }

    [TestMethod]
    public async Task Cancel_PromotesWaitListed_PublishesPromotionEvent()
    {
        var r1 = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");
        var waitlisted = await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com");

        _dispatcher.Dispatched.Clear();

        var cancelResult = await _cancelUseCase.ExecuteAsync(r1.Registration.Id);

        Assert.IsTrue(cancelResult.IsSuccess);
        Assert.IsNotNull(cancelResult.PromotedRegistration);
        Assert.AreEqual(1, _dispatcher.Dispatched.Count);
        var ev = _dispatcher.Dispatched[0] as ParticipantPromotedFromWaitListEvent;
        Assert.IsNotNull(ev);
        Assert.AreEqual(waitlisted.Registration.Id, ev!.RegistrationId);
        Assert.AreEqual("次郎", ev.ParticipantName);
        Assert.AreEqual("jiro@example.com", ev.ParticipantEmail);
    }

    [TestMethod]
    public async Task Cancel_NoWaitListed_DoesNotPublishPromotionEvent()
    {
        var r1 = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        _dispatcher.Dispatched.Clear();

        var cancelResult = await _cancelUseCase.ExecuteAsync(r1.Registration.Id);

        Assert.IsTrue(cancelResult.IsSuccess);
        Assert.IsNull(cancelResult.PromotedRegistration);
        Assert.AreEqual(0, _dispatcher.Dispatched.Count);
    }

    [TestMethod]
    public async Task Cancel_OfWaitListed_DoesNotPublishPromotionEvent()
    {
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");
        var waitlisted = await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com");

        _dispatcher.Dispatched.Clear();

        var cancelResult = await _cancelUseCase.ExecuteAsync(waitlisted.Registration.Id);

        Assert.IsTrue(cancelResult.IsSuccess);
        Assert.IsNull(cancelResult.PromotedRegistration);
        Assert.AreEqual(0, _dispatcher.Dispatched.Count);
    }

    private sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
    {
        public List<IDomainEvent> Dispatched { get; } = [];

        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Dispatched.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            Dispatched.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class TestEventCapacityChecker(EventsDbContext eventsDbContext) : IEventCapacityChecker
    {
        public async Task<EventCapacityInfo?> GetEventCapacityInfoAsync(
            Guid eventId, CancellationToken cancellationToken = default)
        {
            var ev = await eventsDbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            return ev is null ? null : new EventCapacityInfo(ev.Id, ev.Name, ev.Capacity);
        }
    }
}
