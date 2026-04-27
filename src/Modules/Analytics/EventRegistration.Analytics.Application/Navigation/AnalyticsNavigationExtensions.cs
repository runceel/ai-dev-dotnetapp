using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Analytics.Application.Navigation;

/// <summary>
/// Analytics モジュールのナビゲーション項目を DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class AnalyticsNavigationExtensions
{
    /// <summary>
    /// Analytics モジュールが提供するナビゲーション項目を登録する。
    /// </summary>
    public static IServiceCollection AddAnalyticsModuleNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationItem>(new NavigationItem(
            Title: "統計レポート",
            Href: "/analytics",
            Icon: "Analytics",
            Group: "分析",
            Order: 100,
            Match: NavigationMatch.Prefix));

        return services;
    }
}
