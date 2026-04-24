using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Domain;

namespace EventRegistration.Registrations.Application.UseCases;

/// <summary>
/// イベントに対する参加登録一覧を取得するユースケース。
/// </summary>
public sealed class GetRegistrationsByEventUseCase(IRegistrationRepository registrationRepository)
{
    public async Task<IReadOnlyList<Registration>> ExecuteAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await registrationRepository.GetByEventIdAsync(eventId, cancellationToken);
    }
}
