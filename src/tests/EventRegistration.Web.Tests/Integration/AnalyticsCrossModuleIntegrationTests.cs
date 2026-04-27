using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Domain;
using EventRegistration.Analytics.Infrastructure;
using EventRegistration.Events.Domain;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Infrastructure;
using EventRegistration.Web.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Integration;

/// <summary>
/// Registrations モジュールのドメインイベント発行 → Analytics モジュールでの集計反映までを通しで検証する。
/// </summary>
[TestClass]
public sealed class AnalyticsCrossModuleIntegrationTests
{
    private ServiceProvider _provider = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventsModuleInfrastructure();
        services.AddRegistrationsModuleInfrastructure();
        services.AddAnalyticsModule();
        services.AddScoped<IEventCapacityChecker, EventCapacityCheckerAdapter>();

        _provider = services.BuildServiceProvider();

        // 共有 InMemory DB（Events/Registrations/Analytics）をクリーンアップ
        await using var scope = _provider.CreateAsyncScope();
        var eventsDb = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        var regsDb = scope.ServiceProvider.GetRequiredService<EventRegistration.Registrations.Infrastructure.Persistence.RegistrationsDbContext>();
        var analyticsDb = scope.ServiceProvider.GetRequiredService<EventRegistration.Analytics.Infrastructure.Persistence.AnalyticsDbContext>();
        await eventsDb.Database.EnsureDeletedAsync();
        await regsDb.Database.EnsureDeletedAsync();
        await analyticsDb.Database.EnsureDeletedAsync();
    }

    [TestCleanup]
    public void Cleanup() => _provider.Dispose();

    [TestMethod]
    public async Task RegisterAndCancel_FlowIsRecordedInAnalytics()
    {
        Guid eventId;

        // イベント作成（定員 2）
        await using (var scope = _provider.CreateAsyncScope())
        {
            var eventsDb = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
            var ev = Event.Create("統合テストイベント", null, DateTimeOffset.UtcNow.AddDays(7), capacity: 2);
            eventId = ev.Id;
            eventsDb.Events.Add(ev);
            await eventsDb.SaveChangesAsync();
        }

        // 1) 確定登録 2 件
        Guid r1Id, r3Id;
        await using (var scope = _provider.CreateAsyncScope())
        {
            var register = scope.ServiceProvider.GetRequiredService<RegisterParticipantUseCase>();
            var r1 = await register.ExecuteAsync(eventId, "太郎", "taro@example.com");
            r1Id = r1.Registration.Id;
            await register.ExecuteAsync(eventId, "花子", "hanako@example.com");
            // 3) キャンセル待ち登録
            var r3 = await register.ExecuteAsync(eventId, "次郎", "jiro@example.com");
            r3Id = r3.Registration.Id;
        }

        // 4) 太郎をキャンセル → 次郎が繰り上がる
        await using (var scope = _provider.CreateAsyncScope())
        {
            var cancel = scope.ServiceProvider.GetRequiredService<CancelRegistrationUseCase>();
            var result = await cancel.ExecuteAsync(r1Id);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.PromotedRegistration);
            Assert.AreEqual(r3Id, result.PromotedRegistration!.Id);
        }

        // 5) Analytics 集計を確認
        await using (var scope = _provider.CreateAsyncScope())
        {
            var stats = await scope.ServiceProvider
                .GetRequiredService<GetEventStatisticsUseCase>()
                .ExecuteAsync(eventId);

            Assert.AreEqual(2, stats.ConfirmedCount, "新規 Confirmed は 2 件");
            Assert.AreEqual(1, stats.WaitListedCount, "新規 WaitListed は 1 件");
            Assert.AreEqual(1, stats.CancelledCount, "Cancelled は 1 件");
            Assert.AreEqual(1, stats.PromotedCount, "PromotedFromWaitList は 1 件");
            Assert.AreEqual(3, stats.TotalRegistrations);
            // 2 + 1 - 1 = 2
            Assert.AreEqual(2, stats.FinalConfirmedCount);
        }
    }
}
