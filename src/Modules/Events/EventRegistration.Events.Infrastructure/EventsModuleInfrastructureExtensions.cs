using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// Events モジュールの Infrastructure サービスを DI コンテナに登録する拡張メソッド。
/// </summary>
public static class EventsModuleInfrastructureExtensions
{
    /// <summary>
    /// Events モジュールの DbContext、リポジトリ、ユースケースを登録する。
    /// </summary>
    public static IServiceCollection AddEventsModuleInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseInMemoryDatabase("Events"));

        services.AddScoped<IEventRepository, EventRepository>();

        // ユースケース
        services.AddScoped<CreateEventUseCase>();
        services.AddScoped<GetEventsUseCase>();
        services.AddScoped<GetEventByIdUseCase>();

        // TimeProvider（未登録の場合のみ追加）
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
