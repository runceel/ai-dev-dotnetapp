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
    /// <remarks>
    /// Registrations モジュールは独立したページ（/registrations 等）を持たず、
    /// 参加登録機能はイベント詳細画面（/events/{EventId}）内で提供される。
    /// そのため、独立したサイドメニュー項目は登録しない。
    /// 将来 /registrations 専用ページを追加した場合は、このメソッドで項目を登録する。
    /// </remarks>
    public static IServiceCollection AddRegistrationsModuleNavigation(this IServiceCollection services)
    {
        return services;
    }
}
