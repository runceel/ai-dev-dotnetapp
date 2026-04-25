using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Registrations.Application.UseCases;

/// <summary>
/// キャンセルの結果。
/// </summary>
public sealed record CancelResult(bool IsSuccess, string? ErrorMessage = null, Registration? PromotedRegistration = null)
{
    public static CancelResult Success(Registration? promoted = null) => new(true, PromotedRegistration: promoted);
    public static CancelResult Failure(string errorMessage) => new(false, errorMessage);
}

/// <summary>
/// 参加登録をキャンセルするユースケース。
/// キャンセル待ち繰り上げロジックを含む。
/// </summary>
public sealed class CancelRegistrationUseCase(
    IRegistrationRepository registrationRepository,
    IDomainEventDispatcher domainEventDispatcher)
{
    public async Task<CancelResult> ExecuteAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        var registration = await registrationRepository.GetByIdAsync(registrationId, cancellationToken);
        if (registration is null)
        {
            return CancelResult.Failure("指定された登録が見つかりません。");
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            return CancelResult.Failure("既にキャンセル済みです。");
        }

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;
        registration.Cancel();

        // 確定者がキャンセルした場合、キャンセル待ちの先頭を繰り上げ
        Registration? promoted = null;
        if (wasConfirmed)
        {
            var nextWaitListed = await registrationRepository.GetOldestWaitListedAsync(
                registration.EventId, cancellationToken);

            if (nextWaitListed is not null)
            {
                nextWaitListed.Confirm();
                promoted = nextWaitListed;
            }
        }

        await registrationRepository.SaveChangesAsync(cancellationToken);

        // 永続化が成功した場合のみ繰り上げ通知イベントを発行する。
        if (promoted is not null)
        {
            var domainEvent = new ParticipantPromotedFromWaitListEvent(
                RegistrationId: promoted.Id,
                EventId: promoted.EventId,
                ParticipantName: promoted.ParticipantName,
                ParticipantEmail: promoted.Email,
                OccurredAt: DateTimeOffset.UtcNow);

            await domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return CancelResult.Success(promoted);
    }
}
