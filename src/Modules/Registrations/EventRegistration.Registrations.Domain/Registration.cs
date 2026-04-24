namespace EventRegistration.Registrations.Domain;

/// <summary>
/// 参加登録情報を表す集約ルート。
/// </summary>
public class Registration
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string ParticipantName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public RegistrationStatus Status { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private Registration() { }

    public static Registration Create(
        Guid eventId,
        string participantName,
        string email,
        RegistrationStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(participantName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Registration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            ParticipantName = participantName.Trim(),
            Email = NormalizeEmail(email),
            Status = status,
            RegisteredAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// 登録をキャンセルする。
    /// </summary>
    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
        {
            throw new InvalidOperationException("既にキャンセル済みです。");
        }

        Status = RegistrationStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// キャンセル待ちから参加確定に繰り上げる。
    /// </summary>
    public void Confirm()
    {
        if (Status != RegistrationStatus.WaitListed)
        {
            throw new InvalidOperationException("キャンセル待ち状態のみ確定に変更できます。");
        }

        Status = RegistrationStatus.Confirmed;
    }

    /// <summary>
    /// メールアドレスを正規化する（トリム＋小文字化）。
    /// </summary>
    public static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
