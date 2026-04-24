using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Integration;

[TestClass]
public sealed class RegistrationIntegrationTests
{
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;
    private RegisterParticipantUseCase _registerUseCase = null!;
    private CancelRegistrationUseCase _cancelUseCase = null!;
    private GetRegistrationsByEventUseCase _getRegistrationsUseCase = null!;
    private EventsDbContext _eventsDb = null!;
    private Guid _eventId;

    [TestInitialize]
    public async Task Setup()
    {
        var dbId = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddDbContext<EventsDbContext>(o =>
            o.UseInMemoryDatabase($"Events-{dbId}"));
        services.AddDbContext<RegistrationsDbContext>(o =>
            o.UseInMemoryDatabase($"Registrations-{dbId}"));

        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IEventCapacityChecker, TestEventCapacityChecker>();
        services.AddScoped<RegisterParticipantUseCase>();
        services.AddScoped<CancelRegistrationUseCase>();
        services.AddScoped<GetRegistrationsByEventUseCase>();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();

        _eventsDb = _scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        _registerUseCase = _scope.ServiceProvider.GetRequiredService<RegisterParticipantUseCase>();
        _cancelUseCase = _scope.ServiceProvider.GetRequiredService<CancelRegistrationUseCase>();
        _getRegistrationsUseCase = _scope.ServiceProvider.GetRequiredService<GetRegistrationsByEventUseCase>();

        // テスト用イベント作成（定員 2 名）
        var ev = Event.Create("テストイベント", "テスト用", DateTimeOffset.UtcNow.AddDays(7), 2);
        _eventId = ev.Id;
        _eventsDb.Events.Add(ev);
        await _eventsDb.SaveChangesAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [TestMethod]
    public async Task Register_WithinCapacity_StatusIsConfirmed()
    {
        var result = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(RegistrationStatus.Confirmed, result.Registration.Status);
    }

    [TestMethod]
    public async Task Register_ExceedsCapacity_StatusIsWaitListed()
    {
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");

        var result = await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(RegistrationStatus.WaitListed, result.Registration.Status);
    }

    [TestMethod]
    public async Task Register_DuplicateActiveEmail_ReturnsFailure()
    {
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");

        var result = await _registerUseCase.ExecuteAsync(_eventId, "太郎2", "taro@example.com");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task Register_DuplicateEmailDifferentCase_ReturnsFailure()
    {
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");

        var result = await _registerUseCase.ExecuteAsync(_eventId, "太郎2", "TARO@EXAMPLE.COM");

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task Register_AfterCancel_Succeeds()
    {
        var first = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _cancelUseCase.ExecuteAsync(first.Registration.Id);

        var result = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(RegistrationStatus.Confirmed, result.Registration.Status);
    }

    [TestMethod]
    public async Task Register_NonExistentEvent_ReturnsFailure()
    {
        var result = await _registerUseCase.ExecuteAsync(Guid.NewGuid(), "太郎", "taro@example.com");

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task Cancel_ConfirmedWithWaitList_PromotesOldestWaitListed()
    {
        var r1 = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com"); // WaitListed

        var cancelResult = await _cancelUseCase.ExecuteAsync(r1.Registration.Id);

        Assert.IsTrue(cancelResult.IsSuccess);
        Assert.IsNotNull(cancelResult.PromotedRegistration);
        Assert.AreEqual("jiro@example.com", cancelResult.PromotedRegistration.Email);
        Assert.AreEqual(RegistrationStatus.Confirmed, cancelResult.PromotedRegistration.Status);
    }

    [TestMethod]
    public async Task Cancel_WaitListedNoPromotion()
    {
        await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");
        var waitlisted = await _registerUseCase.ExecuteAsync(_eventId, "次郎", "jiro@example.com");

        var cancelResult = await _cancelUseCase.ExecuteAsync(waitlisted.Registration.Id);

        Assert.IsTrue(cancelResult.IsSuccess);
        Assert.IsNull(cancelResult.PromotedRegistration);
    }

    [TestMethod]
    public async Task Cancel_AlreadyCancelled_ReturnsFailure()
    {
        var r = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _cancelUseCase.ExecuteAsync(r.Registration.Id);

        var result = await _cancelUseCase.ExecuteAsync(r.Registration.Id);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task Cancel_NonExistentRegistration_ReturnsFailure()
    {
        var result = await _cancelUseCase.ExecuteAsync(Guid.NewGuid());

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetRegistrations_ExcludesCancelled()
    {
        var r = await _registerUseCase.ExecuteAsync(_eventId, "太郎", "taro@example.com");
        await _registerUseCase.ExecuteAsync(_eventId, "花子", "hanako@example.com");
        await _cancelUseCase.ExecuteAsync(r.Registration.Id);

        var registrations = await _getRegistrationsUseCase.ExecuteAsync(_eventId);

        Assert.AreEqual(1, registrations.Count);
        Assert.AreEqual("hanako@example.com", registrations[0].Email);
    }

    /// <summary>
    /// テスト用の IEventCapacityChecker 実装。EventsDbContext を直接参照する。
    /// </summary>
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
