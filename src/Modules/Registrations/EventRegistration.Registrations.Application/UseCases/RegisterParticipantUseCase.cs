using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Registrations.Application.UseCases;

/// <summary>
/// 参加登録の結果。
/// </summary>
public sealed record RegisterResult(Registration Registration, bool IsSuccess, string? ErrorMessage = null)
{
    public static RegisterResult Success(Registration registration) => new(registration, true);
    public static RegisterResult Failure(string errorMessage) => new(null!, false, errorMessage);
}

/// <summary>
/// イベントへの参加登録を行うユースケース。
/// </summary>
public sealed class RegisterParticipantUseCase(
    IRegistrationRepository registrationRepository,
    IEventCapacityChecker eventCapacityChecker,
    IDomainEventDispatcher domainEventDispatcher)
{
    public async Task<RegisterResult> ExecuteAsync(
        Guid eventId,
        string participantName,
        string email,
        CancellationToken cancellationToken = default)
    {
        // イベントの存在確認と定員取得
        var eventInfo = await eventCapacityChecker.GetEventCapacityInfoAsync(eventId, cancellationToken);
        if (eventInfo is null)
        {
            return RegisterResult.Failure("指定されたイベントが見つかりません。");
        }

        // メールアドレスを正規化
        var normalizedEmail = Registration.NormalizeEmail(email);

        // 重複登録チェック（有効な登録が既にある場合は不可）
        var hasActive = await registrationRepository.HasActiveRegistrationAsync(eventId, normalizedEmail, cancellationToken);
        if (hasActive)
        {
            return RegisterResult.Failure("このメールアドレスでは既に登録済みです。");
        }

        // 定員チェック: Confirmed 数が定員未満なら Confirmed、以上なら WaitListed
        var confirmedCount = await registrationRepository.CountConfirmedByEventIdAsync(eventId, cancellationToken);
        var status = confirmedCount < eventInfo.Capacity
            ? RegistrationStatus.Confirmed
            : RegistrationStatus.WaitListed;

        var registration = Registration.Create(eventId, participantName, email, status);
        await registrationRepository.AddAsync(registration, cancellationToken);
        await registrationRepository.SaveChangesAsync(cancellationToken);

        // 永続化が成功した場合のみ、参加確定の通知イベントを発行する。
        if (registration.Status == RegistrationStatus.Confirmed)
        {
            var domainEvent = new ParticipantConfirmedEvent(
                RegistrationId: registration.Id,
                EventId: registration.EventId,
                ParticipantName: registration.ParticipantName,
                ParticipantEmail: registration.Email,
                OccurredAt: DateTimeOffset.UtcNow);

            await domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return RegisterResult.Success(registration);
    }
}
