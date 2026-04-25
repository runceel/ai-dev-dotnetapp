using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;
using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.Web.Components.Pages.Events;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Events;

[TestClass]
public sealed class EventDetailTests : BunitContext
{
    private IEventRepository _mockEventRepo = default!;
    private IRegistrationRepository _mockRegRepo = default!;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepo = Substitute.For<IEventRepository>();
        _mockRegRepo = Substitute.For<IRegistrationRepository>();

        Services.AddSingleton(_mockEventRepo);
        Services.AddSingleton(_mockRegRepo);
        Services.AddTransient<GetEventByIdUseCase>();
        Services.AddTransient<GetRegistrationsByEventUseCase>();

        // RegistrationForm の依存 (子コンポーネント)
        var mockCapacity = Substitute.For<IEventCapacityChecker>();
        Services.AddSingleton(mockCapacity);
        Services.AddTransient<RegisterParticipantUseCase>();

        // ParticipantList の依存 (子コンポーネント)
        Services.AddTransient<CancelRegistrationUseCase>();

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void EventNotFound_ShowsWarning()
    {
        var eventId = Guid.NewGuid();
        _mockEventRepo.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        var cut = Render<EventDetail>(p => p
            .Add(x => x.EventId, eventId));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("イベントが見つかりません")));
    }

    [TestMethod]
    public void EventNotFound_ShowsBackLink()
    {
        var eventId = Guid.NewGuid();
        _mockEventRepo.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        var cut = Render<EventDetail>(p => p
            .Add(x => x.EventId, eventId));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("一覧に戻る")));
    }

    [TestMethod]
    public void EventFound_ShowsEventName()
    {
        var ev = Event.Create("テストイベント名", "説明文", DateTimeOffset.UtcNow.AddDays(7), 30);
        _mockEventRepo.GetByIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns(ev);
        _mockRegRepo.GetByEventIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)new List<Registration>());

        var cut = Render<EventDetail>(p => p
            .Add(x => x.EventId, ev.Id));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("テストイベント名")));
    }

    [TestMethod]
    public void EventFound_ShowsCapacityAndRemainingSlots()
    {
        var ev = Event.Create("テスト", null, DateTimeOffset.UtcNow.AddDays(7), 10);
        var registrations = new List<Registration>
        {
            Registration.Create(ev.Id, "太郎", "taro@example.com", RegistrationStatus.Confirmed),
            Registration.Create(ev.Id, "花子", "hanako@example.com", RegistrationStatus.Confirmed),
            Registration.Create(ev.Id, "次郎", "jiro@example.com", RegistrationStatus.WaitListed),
        };
        _mockEventRepo.GetByIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns(ev);
        _mockRegRepo.GetByEventIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<EventDetail>(p => p
            .Add(x => x.EventId, ev.Id));

        // Capacity=10, Confirmed=2 → remaining=8
        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("10"));
            Assert.IsTrue(cut.Markup.Contains("8"));
        });
    }

    [TestMethod]
    public void EventFound_ShowsDescription()
    {
        var ev = Event.Create("テスト", "詳細な説明テキスト", DateTimeOffset.UtcNow.AddDays(7), 5);
        _mockEventRepo.GetByIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns(ev);
        _mockRegRepo.GetByEventIdAsync(ev.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)new List<Registration>());

        var cut = Render<EventDetail>(p => p
            .Add(x => x.EventId, ev.Id));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("詳細な説明テキスト")));
    }
}
