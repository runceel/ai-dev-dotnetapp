using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Domain;
using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;
using EventRegistration.Web.Components.Pages.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Analytics;

[TestClass]
public sealed class AnalyticsListTests : BunitContext
{
    private IEventRepository _mockEventRepo = default!;
    private IRegistrationActivityRepository _mockActivityRepo = default!;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepo = Substitute.For<IEventRepository>();
        _mockActivityRepo = Substitute.For<IRegistrationActivityRepository>();
        Services.AddSingleton(_mockEventRepo);
        Services.AddSingleton(_mockActivityRepo);
        Services.AddTransient<GetAllEventsUseCase>();
        Services.AddTransient<GetEventStatisticsUseCase>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void NoEvents_ShowsEmptyMessage()
    {
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event>());

        var cut = Render<AnalyticsList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("集計対象のイベントがまだありません")));
    }

    [TestMethod]
    public void WithEvents_ShowsEventNamesAndStatistics()
    {
        var ev = Event.Create("分析対象イベント", null, DateTimeOffset.UtcNow.AddDays(7), 10);
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event> { ev });

        _mockActivityRepo.GetEventStatisticsAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns(new EventStatistics(ev.Id, ConfirmedCount: 5, WaitListedCount: 2, CancelledCount: 1, PromotedCount: 1));

        var cut = Render<AnalyticsList>();

        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("分析対象イベント"));
            // TotalRegistrations = 5 + 2 = 7
            Assert.IsTrue(cut.Markup.Contains(">7<"));
        });
    }
}
