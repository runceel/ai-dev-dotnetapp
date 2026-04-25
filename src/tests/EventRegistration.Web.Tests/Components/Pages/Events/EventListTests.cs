using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;
using EventRegistration.Web.Components.Pages.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Events;

[TestClass]
public sealed class EventListTests : BunitContext
{
    private IEventRepository _mockEventRepo = default!;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepo = Substitute.For<IEventRepository>();
        Services.AddSingleton(_mockEventRepo);
        Services.AddTransient<GetAllEventsUseCase>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void NoEvents_ShowsEmptyMessage()
    {
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event>());

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("イベントがまだ登録されていません")));
    }

    [TestMethod]
    public void WithEvents_ShowsEventNames()
    {
        var events = new List<Event>
        {
            Event.Create("テストイベント1", "説明1", DateTimeOffset.UtcNow.AddDays(7), 10),
            Event.Create("テストイベント2", null, DateTimeOffset.UtcNow.AddDays(14), 20),
        };
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)events);

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("テストイベント1"));
            Assert.IsTrue(cut.Markup.Contains("テストイベント2"));
        });
    }

    [TestMethod]
    public void WithEvents_ShowsCapacity()
    {
        var ev = Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 50);
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event> { ev });

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("50")));
    }

    [TestMethod]
    public void WithEvents_ShowsDescription()
    {
        var ev = Event.Create("テスト", "イベントの詳細説明テキスト", DateTimeOffset.UtcNow.AddDays(7), 10);
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event> { ev });

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("イベントの詳細説明テキスト")));
    }

    [TestMethod]
    public void ClickEventCard_NavigatesToDetail()
    {
        var ev = Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 10);
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event> { ev });

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("テスト")));

        cut.Find(".mud-card").Click();

        var navMan = Services.GetRequiredService<NavigationManager>();
        Assert.IsTrue(navMan.Uri.Contains($"/events/{ev.Id}"));
    }

    [TestMethod]
    public void ShowsCreateButton()
    {
        _mockEventRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>)new List<Event>());

        var cut = Render<EventList>();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("新しいイベントを作成")));
    }
}
