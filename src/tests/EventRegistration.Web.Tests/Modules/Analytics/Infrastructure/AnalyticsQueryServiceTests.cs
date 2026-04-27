using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Infrastructure;
using EventRegistration.Analytics.Infrastructure.Queries;
using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Analytics.Infrastructure;

[TestClass]
public sealed class AnalyticsModuleInfrastructureTests
{
    [TestMethod]
    public void AddAnalyticsModuleInfrastructure_RegistersQueryServiceAndUseCases()
    {
        var services = new ServiceCollection();

        services.AddAnalyticsModuleInfrastructure();

        var queryDescriptor = services.Single(d => d.ServiceType == typeof(IAnalyticsQueryService));
        Assert.AreEqual(ServiceLifetime.Scoped, queryDescriptor.Lifetime);

        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetEventStatisticsUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetOverallSummaryUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetDailyRegistrationTrendUseCase)));
    }

    [TestMethod]
    public void AddAnalyticsModuleInfrastructure_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddAnalyticsModuleInfrastructure();
        Assert.AreSame(services, result);
    }
}

[TestClass]
public sealed class AnalyticsQueryServiceTests
{
    private static (EventsDbContext events, RegistrationsDbContext regs) CreateContexts(string suffix)
    {
        var eventsOpts = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(databaseName: $"AnalyticsTests_Events_{suffix}")
            .Options;
        var regsOpts = new DbContextOptionsBuilder<RegistrationsDbContext>()
            .UseInMemoryDatabase(databaseName: $"AnalyticsTests_Regs_{suffix}")
            .Options;
        return (new EventsDbContext(eventsOpts), new RegistrationsDbContext(regsOpts));
    }

    private static Registration MakeRegistration(Guid eventId, RegistrationStatus status, string emailLocal = "u")
    {
        // Confirmed/WaitListed のみ生成可能
        var initialStatus = status == RegistrationStatus.Cancelled ? RegistrationStatus.Confirmed : status;
        var reg = Registration.Create(eventId, "User", $"{emailLocal}@example.com", initialStatus);
        if (status == RegistrationStatus.Cancelled)
        {
            reg.Cancel();
        }
        return reg;
    }

    [TestMethod]
    public async Task GetEventStatisticsAsync_ReturnsAllEventsWithCounts()
    {
        var (events, regs) = CreateContexts(nameof(GetEventStatisticsAsync_ReturnsAllEventsWithCounts));

        var ev1 = Event.Create("E1", null, DateTimeOffset.UtcNow.AddDays(10), 5);
        var ev2 = Event.Create("E2", null, DateTimeOffset.UtcNow.AddDays(20), 3);
        events.Events.AddRange(ev1, ev2);
        await events.SaveChangesAsync();

        regs.Registrations.AddRange(
            MakeRegistration(ev1.Id, RegistrationStatus.Confirmed, "a"),
            MakeRegistration(ev1.Id, RegistrationStatus.Confirmed, "b"),
            MakeRegistration(ev1.Id, RegistrationStatus.WaitListed, "c"),
            MakeRegistration(ev1.Id, RegistrationStatus.Cancelled, "d"),
            MakeRegistration(ev2.Id, RegistrationStatus.Confirmed, "e"));
        await regs.SaveChangesAsync();

        var sut = new AnalyticsQueryService(events, regs);

        var result = await sut.GetEventStatisticsAsync();

        Assert.AreEqual(2, result.Count);
        var s1 = result.Single(s => s.EventId == ev1.Id);
        Assert.AreEqual("E1", s1.EventName);
        Assert.AreEqual(5, s1.Capacity);
        Assert.AreEqual(2, s1.ConfirmedCount);
        Assert.AreEqual(1, s1.WaitListedCount);
        Assert.AreEqual(1, s1.CancelledCount);
        Assert.AreEqual(0.4d, s1.ParticipationRate, 1e-9);

        var s2 = result.Single(s => s.EventId == ev2.Id);
        Assert.AreEqual(1, s2.ConfirmedCount);
        Assert.AreEqual(0, s2.WaitListedCount);
        Assert.AreEqual(0, s2.CancelledCount);
    }

    [TestMethod]
    public async Task GetEventStatisticsAsync_NoEvents_ReturnsEmpty()
    {
        var (events, regs) = CreateContexts(nameof(GetEventStatisticsAsync_NoEvents_ReturnsEmpty));
        var sut = new AnalyticsQueryService(events, regs);

        var result = await sut.GetEventStatisticsAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetOverallSummaryAsync_AggregatesAcrossEvents()
    {
        var (events, regs) = CreateContexts(nameof(GetOverallSummaryAsync_AggregatesAcrossEvents));

        var ev1 = Event.Create("E1", null, DateTimeOffset.UtcNow, 10);
        var ev2 = Event.Create("E2", null, DateTimeOffset.UtcNow, 10);
        events.Events.AddRange(ev1, ev2);
        await events.SaveChangesAsync();

        regs.Registrations.AddRange(
            MakeRegistration(ev1.Id, RegistrationStatus.Confirmed, "a"),
            MakeRegistration(ev1.Id, RegistrationStatus.WaitListed, "b"),
            MakeRegistration(ev2.Id, RegistrationStatus.Cancelled, "c"),
            MakeRegistration(ev2.Id, RegistrationStatus.Cancelled, "d"));
        await regs.SaveChangesAsync();

        var sut = new AnalyticsQueryService(events, regs);

        var summary = await sut.GetOverallSummaryAsync();

        Assert.AreEqual(2, summary.TotalEvents);
        Assert.AreEqual(1, summary.TotalConfirmed);
        Assert.AreEqual(1, summary.TotalWaitListed);
        Assert.AreEqual(2, summary.TotalCancelled);
        Assert.AreEqual(4, summary.TotalRegistrations);
        Assert.AreEqual(0.5d, summary.OverallCancellationRate, 1e-9);
    }

    [TestMethod]
    public async Task GetDailyRegistrationTrendAsync_ReturnsContiguousDailyPoints()
    {
        var (events, regs) = CreateContexts(nameof(GetDailyRegistrationTrendAsync_ReturnsContiguousDailyPoints));

        var ev = Event.Create("E", null, DateTimeOffset.UtcNow, 10);
        events.Events.Add(ev);
        await events.SaveChangesAsync();

        // 今日と昨日の登録 + 昨日の 1 件をキャンセル
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var r1 = MakeRegistration(ev.Id, RegistrationStatus.Confirmed, "a");
        var r2 = MakeRegistration(ev.Id, RegistrationStatus.Confirmed, "b");
        var r3 = MakeRegistration(ev.Id, RegistrationStatus.Cancelled, "c");
        regs.Registrations.AddRange(r1, r2, r3);
        await regs.SaveChangesAsync();

        var sut = new AnalyticsQueryService(events, regs);

        var from = today.AddDays(-2);
        var to = today;
        var result = await sut.GetDailyRegistrationTrendAsync(from, to);

        // 3 日分のポイントを返す（連続性チェック）
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(from, result[0].Date);
        Assert.AreEqual(from.AddDays(1), result[1].Date);
        Assert.AreEqual(to, result[2].Date);

        // 今日付近に少なくとも r1/r2/r3 の 3 件の登録イベントが集計されている
        var totalReg = result.Sum(p => p.RegistrationCount);
        Assert.AreEqual(3, totalReg);

        // r3 のキャンセル件数が 1 含まれる
        var totalCancel = result.Sum(p => p.CancellationCount);
        Assert.AreEqual(1, totalCancel);
    }

    [TestMethod]
    public async Task GetDailyRegistrationTrend_UseCase_RejectsInvertedRange()
    {
        var (events, regs) = CreateContexts(nameof(GetDailyRegistrationTrend_UseCase_RejectsInvertedRange));
        var sut = new GetDailyRegistrationTrendUseCase(new AnalyticsQueryService(events, regs));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => sut.ExecuteAsync(today, today.AddDays(-1)));
    }
}
