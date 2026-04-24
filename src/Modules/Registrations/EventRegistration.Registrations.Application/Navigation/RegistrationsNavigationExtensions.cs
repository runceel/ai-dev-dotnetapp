using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Registrations.Application.Navigation;

/// <summary>
/// Registrations モジュールのナビゲーション項目を DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class RegistrationsNavigationExtensions
{
    /// <summary>
    /// Registrations モジュールが提供するナビゲーション項目を登録する。
    /// </summary>
    public static IServiceCollection AddRegistrationsModuleNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationItem>(new NavigationItem(
            Title: "参加登録",
            Href: "/", // プレースホルダー (将来 /registrations 等に差し替え)
            Icon: "HowToReg",
            Group: "参加者",
            Order: 200,
            Match: NavigationMatch.Prefix));

        return services;
    }
}
