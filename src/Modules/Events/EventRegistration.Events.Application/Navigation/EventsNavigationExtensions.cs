using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Events.Application.Navigation;

/// <summary>
/// Events モジュールのナビゲーション項目を DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class EventsNavigationExtensions
{
    /// <summary>
    /// Events モジュールが提供するナビゲーション項目を登録する。
    /// </summary>
    public static IServiceCollection AddEventsModuleNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationItem>(new NavigationItem(
            Title: "イベント管理",
            Href: "/events",
            Icon: "Event",
            Group: "イベント",
            Order: 100,
            Match: NavigationMatch.Prefix));

        return services;
    }
}
