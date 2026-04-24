using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Events.Application;

/// <summary>
/// Events モジュールの Application 層サービスを DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class EventsApplicationExtensions
{
    /// <summary>
    /// Events モジュールのアプリケーションサービスを登録する。
    /// </summary>
    public static IServiceCollection AddEventsApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventsAppService, EventsAppService>();
        return services;
    }
}
