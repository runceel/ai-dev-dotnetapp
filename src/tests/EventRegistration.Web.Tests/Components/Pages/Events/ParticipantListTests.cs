using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.SharedKernel.Application.Events;
using EventRegistration.Web.Components.Pages.Events;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace EventRegistration.Web.Tests.Components.Pages.Events;

[TestClass]
public sealed class ParticipantListTests : BunitContext
{
    private IRegistrationRepository _mockRegRepo = default!;
    private Guid _eventId;

    [TestInitialize]
    public void Setup()
    {
        _eventId = Guid.NewGuid();
        _mockRegRepo = Substitute.For<IRegistrationRepository>();
        Services.AddSingleton(_mockRegRepo);
        Services.AddTransient<GetRegistrationsByEventUseCase>();
        var mockDispatcher = Substitute.For<IDomainEventDispatcher>();
        Services.AddSingleton(mockDispatcher);
        Services.AddTransient<CancelRegistrationUseCase>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TestMethod]
    public void NoRegistrations_ShowsEmptyMessage()
    {
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)new List<Registration>());

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("まだ参加者がいません")));
    }

    [TestMethod]
    public void WithConfirmedRegistrations_ShowsConfirmedSection()
    {
        var registrations = new List<Registration>
        {
            Registration.Create(_eventId, "太郎", "taro@example.com", RegistrationStatus.Confirmed),
            Registration.Create(_eventId, "花子", "hanako@example.com", RegistrationStatus.Confirmed),
        };
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("参加確定"));
            Assert.IsTrue(cut.Markup.Contains("2 名"));
            Assert.IsTrue(cut.Markup.Contains("太郎"));
            Assert.IsTrue(cut.Markup.Contains("花子"));
        });
    }

    [TestMethod]
    public void WithWaitListedRegistrations_ShowsWaitListedSection()
    {
        var registrations = new List<Registration>
        {
            Registration.Create(_eventId, "次郎", "jiro@example.com", RegistrationStatus.WaitListed),
        };
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("キャンセル待ち"));
            Assert.IsTrue(cut.Markup.Contains("1 名"));
            Assert.IsTrue(cut.Markup.Contains("次郎"));
        });
    }

    [TestMethod]
    public void WithMixedRegistrations_ShowsBothSections()
    {
        var registrations = new List<Registration>
        {
            Registration.Create(_eventId, "太郎", "taro@example.com", RegistrationStatus.Confirmed),
            Registration.Create(_eventId, "次郎", "jiro@example.com", RegistrationStatus.WaitListed),
        };
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
        {
            Assert.IsTrue(cut.Markup.Contains("参加確定"));
            Assert.IsTrue(cut.Markup.Contains("キャンセル待ち"));
            Assert.IsTrue(cut.Markup.Contains("太郎"));
            Assert.IsTrue(cut.Markup.Contains("次郎"));
        });
    }

    [TestMethod]
    public void ShowsEmailAddresses()
    {
        var registrations = new List<Registration>
        {
            Registration.Create(_eventId, "太郎", "taro@example.com", RegistrationStatus.Confirmed),
        };
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("taro@example.com")));
    }

    [TestMethod]
    public void ShowsCancelButtons()
    {
        var registrations = new List<Registration>
        {
            Registration.Create(_eventId, "太郎", "taro@example.com", RegistrationStatus.Confirmed),
        };
        _mockRegRepo.GetByEventIdAsync(_eventId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Registration>)registrations);

        var cut = Render<ParticipantList>(p => p
            .Add(x => x.EventId, _eventId));

        cut.WaitForAssertion(() =>
            Assert.IsTrue(cut.Markup.Contains("キャンセル")));
    }
}
