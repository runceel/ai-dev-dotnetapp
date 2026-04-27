using EventRegistration.Notifications.Application.Handlers;
using EventRegistration.Notifications.Application.Services;
using EventRegistration.Notifications.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Web.Tests.Modules.Notifications;

[TestClass]
public sealed class ParticipantConfirmedNotificationHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_PassesExpectedFieldsToSender()
    {
        var sender = new RecordingNotificationSender();
        var handler = new ParticipantConfirmedNotificationHandler(sender);

        var registrationId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var domainEvent = new ParticipantConfirmedEvent(
            RegistrationId: registrationId,
            EventId: eventId,
            ParticipantName: "太郎",
            ParticipantEmail: "taro@example.com",
            OccurredAt: DateTimeOffset.UtcNow);

        await handler.HandleAsync(domainEvent);

        Assert.IsNotNull(sender.LastMessage);
        Assert.AreEqual(NotificationKind.ParticipantConfirmed, sender.LastMessage!.Kind);
        Assert.AreEqual(eventId, sender.LastMessage.EventId);
        Assert.AreEqual(registrationId, sender.LastMessage.RegistrationId);
        Assert.AreEqual("太郎", sender.LastMessage.ParticipantName);
        Assert.AreEqual("taro@example.com", sender.LastMessage.ParticipantEmail);
    }

    [TestMethod]
    public async Task HandleAsync_NullEvent_Throws()
    {
        var handler = new ParticipantConfirmedNotificationHandler(new RecordingNotificationSender());

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}

[TestClass]
public sealed class ParticipantPromotedFromWaitListNotificationHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_PassesExpectedFieldsToSender()
    {
        var sender = new RecordingNotificationSender();
        var handler = new ParticipantPromotedFromWaitListNotificationHandler(sender);

        var registrationId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var domainEvent = new ParticipantPromotedFromWaitListEvent(
            RegistrationId: registrationId,
            EventId: eventId,
            ParticipantName: "次郎",
            ParticipantEmail: "jiro@example.com",
            OccurredAt: DateTimeOffset.UtcNow);

        await handler.HandleAsync(domainEvent);

        Assert.IsNotNull(sender.LastMessage);
        Assert.AreEqual(NotificationKind.ParticipantPromotedFromWaitList, sender.LastMessage!.Kind);
        Assert.AreEqual(eventId, sender.LastMessage.EventId);
        Assert.AreEqual(registrationId, sender.LastMessage.RegistrationId);
        Assert.AreEqual("次郎", sender.LastMessage.ParticipantName);
        Assert.AreEqual("jiro@example.com", sender.LastMessage.ParticipantEmail);
    }
}

internal sealed class RecordingNotificationSender : INotificationSender
{
    public NotificationMessage? LastMessage { get; private set; }
    public int CallCount { get; private set; }

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        LastMessage = message;
        CallCount++;
        return Task.CompletedTask;
    }
}
