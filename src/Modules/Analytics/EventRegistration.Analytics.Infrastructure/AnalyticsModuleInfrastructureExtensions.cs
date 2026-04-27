using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Infrastructure.Handlers;
using EventRegistration.Analytics.Infrastructure.Persistence;
using EventRegistration.SharedKernel.Application.Events;
using EventRegistration.SharedKernel.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Analytics.Infrastructure;

/// <summary>
/// Analytics モジュールの DI 登録を集約する拡張メソッド。
/// </summary>
public static class AnalyticsModuleInfrastructureExtensions
{
    /// <summary>
    /// Analytics モジュールの DbContext、リポジトリ、ユースケース、ドメインイベントハンドラを登録する。
    /// </summary>
    /// <remarks>
    /// SharedKernel が提供する既定の <see cref="IDomainEventDispatcher"/> も併せて登録する
    /// （他のモジュールから既に登録済みの場合は <c>TryAdd</c> によりスキップ）。
    /// 配線完結のため Composition Root では <c>AddAnalyticsModule()</c> 1 行で済む。
    /// </remarks>
    public static IServiceCollection AddAnalyticsModule(this IServiceCollection services)
    {
        services.AddSharedKernelDomainEvents();

        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "Analytics"));

        services.AddScoped<IRegistrationActivityRepository, RegistrationActivityRepository>();
        services.AddScoped<GetEventStatisticsUseCase>();
        services.AddScoped<GetDailyStatisticsUseCase>();

        // ドメインイベント購読
        services.AddScoped<IDomainEventHandler<ParticipantConfirmedEvent>, ParticipantConfirmedAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<ParticipantWaitListedEvent>, ParticipantWaitListedAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<RegistrationCancelledEvent>, RegistrationCancelledAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<ParticipantPromotedFromWaitListEvent>, ParticipantPromotedFromWaitListAnalyticsHandler>();

        return services;
    }
}
