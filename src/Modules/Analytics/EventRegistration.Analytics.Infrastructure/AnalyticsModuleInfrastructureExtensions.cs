using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Analytics.Infrastructure;

/// <summary>
/// Analytics モジュールの Infrastructure サービスを DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class AnalyticsModuleInfrastructureExtensions
{
    /// <summary>
    /// Analytics モジュールのクエリサービスとユースケースを登録する。
    /// </summary>
    /// <remarks>
    /// 本メソッドは <c>AddEventsModuleInfrastructure()</c> および
    /// <c>AddRegistrationsModuleInfrastructure()</c> によって登録される
    /// <c>EventsDbContext</c> / <c>RegistrationsDbContext</c> に依存する。
    /// Composition Root ではこれらを先に登録すること。
    /// </remarks>
    public static IServiceCollection AddAnalyticsModuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddScoped<GetEventStatisticsUseCase>();
        services.AddScoped<GetOverallSummaryUseCase>();
        services.AddScoped<GetDailyRegistrationTrendUseCase>();

        return services;
    }
}
