using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Domain;
using EventRegistration.Web.Components.Pages.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Events;

[TestClass]
public sealed class EventCreateTests : BunitContext
{
    private IEventRepository _mockEventRepo = default!;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepo = Substitute.For<IEventRepository>();
        Services.AddSingleton(_mockEventRepo);
        Services.AddTransient<CreateEventUseCase>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        // MudDatePicker/MudTimePicker が MudPopoverProvider を必要とする
        Render<MudBlazor.MudPopoverProvider>();
    }

    [TestMethod]
    public void InitialRender_ShowsFormTitle()
    {
        var cut = Render<EventCreate>();

        Assert.IsTrue(cut.Markup.Contains("新しいイベントの作成"));
    }

    [TestMethod]
    public void InitialRender_ShowsSaveButton()
    {
        var cut = Render<EventCreate>();

        Assert.IsTrue(cut.Markup.Contains("保存"));
    }

    [TestMethod]
    public void InitialRender_ShowsCancelLink()
    {
        var cut = Render<EventCreate>();

        Assert.IsTrue(cut.Markup.Contains("キャンセル"));
    }

    [TestMethod]
    public void SubmitSuccess_NavigatesToEventDetail()
    {
        _mockEventRepo.AddAsync(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var cut = Render<EventCreate>();

        // Fill in required Name field via MudTextField input
        var inputs = cut.FindAll("input");
        inputs[0].Change("テストイベント");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            var navMan = Services.GetRequiredService<NavigationManager>();
            Assert.IsTrue(navMan.Uri.Contains("/events/"));
        });
    }

    [TestMethod]
    public void SubmitError_ShowsErrorMessage()
    {
        _mockEventRepo.AddAsync(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("データベースエラー")));

        var cut = Render<EventCreate>();

        var inputs = cut.FindAll("input");
        inputs[0].Change("テスト");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("データベースエラー")));
    }
}
