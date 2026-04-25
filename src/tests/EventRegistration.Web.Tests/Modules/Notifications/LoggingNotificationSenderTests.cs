using EventRegistration.Notifications.Domain;
using EventRegistration.Notifications.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;

namespace EventRegistration.Web.Tests.Modules.Notifications;

[TestClass]
public sealed class LoggingNotificationSenderTests
{
    [TestMethod]
    public async Task SendAsync_LogsStructuredFields()
    {
        var logger = new RecordingLogger<LoggingNotificationSender>();
        var sender = new LoggingNotificationSender(logger);

        var eventId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var message = new NotificationMessage(
            Kind: NotificationKind.ParticipantConfirmed,
            EventId: eventId,
            RegistrationId: registrationId,
            ParticipantName: "太郎",
            ParticipantEmail: "taro@example.com");

        await sender.SendAsync(message);

        Assert.AreEqual(1, logger.Entries.Count);
        var entry = logger.Entries[0];
        Assert.AreEqual(LogLevel.Information, entry.Level);

        // 構造化フィールド検証 (AC-06)
        Assert.AreEqual(NotificationKind.ParticipantConfirmed, entry.GetValue<NotificationKind>("Kind"));
        Assert.AreEqual(eventId, entry.GetValue<Guid>("EventId"));
        Assert.AreEqual(registrationId, entry.GetValue<Guid>("RegistrationId"));
        Assert.AreEqual("太郎", entry.GetValue<string>("ParticipantName"));
        Assert.AreEqual("taro@example.com", entry.GetValue<string>("ParticipantEmail"));
    }

    [TestMethod]
    public async Task SendAsync_NullMessage_Throws()
    {
        var sender = new LoggingNotificationSender(new RecordingLogger<LoggingNotificationSender>());

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => sender.SendAsync(null!));
    }
}

internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    IDisposable? ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var values = state as IReadOnlyList<KeyValuePair<string, object?>>;
        Entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception), exception, values));
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

internal sealed record LogEntry(
    LogLevel Level,
    EventId EventId,
    string Message,
    Exception? Exception,
    IReadOnlyList<KeyValuePair<string, object?>>? Values)
{
    public TValue? GetValue<TValue>(string key)
    {
        if (Values is null)
        {
            return default;
        }

        foreach (var kvp in Values)
        {
            if (kvp.Key == key && kvp.Value is TValue typed)
            {
                return typed;
            }
        }

        return default;
    }
}
