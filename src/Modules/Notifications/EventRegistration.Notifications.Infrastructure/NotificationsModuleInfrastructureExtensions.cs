using EventRegistration.Notifications.Application.Handlers;
using EventRegistration.Notifications.Application.Services;
using EventRegistration.Notifications.Infrastructure.Notifications;
using EventRegistration.SharedKernel.Application.Events;
using EventRegistration.SharedKernel.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventRegistration.Notifications.Infrastructure;

/// <summary>
/// Notifications モジュールの DI 登録を集約する拡張メソッド。
/// </summary>
public static class NotificationsModuleInfrastructureExtensions
{
    /// <summary>
    /// Notifications モジュールが提供する送信実装と、購読する <see cref="IDomainEventHandler{TEvent}"/>
    /// を DI コンテナへ登録する。
    /// </summary>
    /// <remarks>
    /// 同時に SharedKernel が提供する既定の <see cref="IDomainEventDispatcher"/> も登録する
    /// （他のモジュールから既に登録済みの場合は <c>TryAdd</c> によりスキップ）。
    /// 配線完結のため Composition Root では <c>AddNotificationsModule()</c> 1 行で済む。
    /// </remarks>
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        // ディスパッチャ（SharedKernel 提供の既定実装）
        services.AddSharedKernelDomainEvents();

        // 送信実装
        services.TryAddScoped<INotificationSender, LoggingNotificationSender>();

        // ハンドラ登録（IEnumerable<IDomainEventHandler<T>> 解決時に取得される）
        services.AddScoped<IDomainEventHandler<ParticipantConfirmedEvent>, ParticipantConfirmedNotificationHandler>();
        services.AddScoped<IDomainEventHandler<ParticipantPromotedFromWaitListEvent>, ParticipantPromotedFromWaitListNotificationHandler>();

        return services;
    }
}
