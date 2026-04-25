using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.SharedKernel.Application.Events;
using EventRegistration.Web.Components.Pages.Events;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Events;

[TestClass]
public sealed class RegistrationFormTests : BunitContext
{
    private IRegistrationRepository _mockRegRepo = default!;
    private IEventCapacityChecker _mockCapacity = default!;
    private Guid _eventId;

    [TestInitialize]
    public void Setup()
    {
        _eventId = Guid.NewGuid();
        _mockRegRepo = Substitute.For<IRegistrationRepository>();
        _mockCapacity = Substitute.For<IEventCapacityChecker>();

        Services.AddSingleton(_mockRegRepo);
        Services.AddSingleton(_mockCapacity);
        var mockDispatcher = Substitute.For<IDomainEventDispatcher>();
        Services.AddSingleton(mockDispatcher);
        Services.AddTransient<RegisterParticipantUseCase>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void InitialRender_ShowsFormElements()
    {
        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        Assert.IsTrue(cut.Markup.Contains("参加登録"));
    }

    [TestMethod]
    public void InitialRender_ShowsNameAndEmailLabels()
    {
        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        Assert.IsTrue(cut.Markup.Contains("参加者名"));
        Assert.IsTrue(cut.Markup.Contains("メールアドレス"));
    }

    [TestMethod]
    public void SubmitSuccess_Confirmed_ShowsSuccessMessage()
    {
        _mockCapacity.GetEventCapacityInfoAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns(new EventCapacityInfo(_eventId, "テスト", 10));
        _mockRegRepo.HasActiveRegistrationAsync(_eventId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockRegRepo.CountConfirmedByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns(0);

        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        // Fill in form fields
        var inputs = cut.FindAll("input");
        inputs[0].Change("テスト太郎");
        inputs[1].Change("taro@example.com");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("参加確定")));
    }

    [TestMethod]
    public void SubmitSuccess_WaitListed_ShowsWarningMessage()
    {
        _mockCapacity.GetEventCapacityInfoAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns(new EventCapacityInfo(_eventId, "テスト", 2));
        _mockRegRepo.HasActiveRegistrationAsync(_eventId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockRegRepo.CountConfirmedByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns(2); // At capacity

        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        var inputs = cut.FindAll("input");
        inputs[0].Change("テスト花子");
        inputs[1].Change("hanako@example.com");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("キャンセル待ち")));
    }

    [TestMethod]
    public void SubmitFailure_DuplicateEmail_ShowsErrorMessage()
    {
        _mockCapacity.GetEventCapacityInfoAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns(new EventCapacityInfo(_eventId, "テスト", 10));
        _mockRegRepo.HasActiveRegistrationAsync(_eventId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true); // Duplicate

        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        var inputs = cut.FindAll("input");
        inputs[0].Change("テスト太郎");
        inputs[1].Change("taro@example.com");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("既に登録済み")));
    }

    [TestMethod]
    public void SubmitFailure_EventNotFound_ShowsErrorMessage()
    {
        _mockCapacity.GetEventCapacityInfoAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((EventCapacityInfo?)null);

        var cut = Render<RegistrationForm>(p => p
            .Add(x => x.EventId, _eventId));

        var inputs = cut.FindAll("input");
        inputs[0].Change("テスト太郎");
        inputs[1].Change("taro@example.com");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("見つかりません")));
    }
}
