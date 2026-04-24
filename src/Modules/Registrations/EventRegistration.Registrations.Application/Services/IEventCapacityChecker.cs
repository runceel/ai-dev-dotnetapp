namespace EventRegistration.Registrations.Application.Services;

/// <summary>
/// イベントの定員情報を取得するための反腐敗層インターフェース。
/// Registrations モジュールが Events モジュールのデータに間接的にアクセスする。
/// </summary>
public interface IEventCapacityChecker
{
    /// <summary>
    /// 指定されたイベントの定員情報を取得する。
    /// </summary>
    /// <returns>イベントが存在する場合はその情報、存在しない場合は null。</returns>
    Task<EventCapacityInfo?> GetEventCapacityInfoAsync(Guid eventId, CancellationToken cancellationToken = default);
}

/// <summary>
/// イベントの定員に関する情報。
/// </summary>
public sealed record EventCapacityInfo(Guid EventId, string EventName, int Capacity);
