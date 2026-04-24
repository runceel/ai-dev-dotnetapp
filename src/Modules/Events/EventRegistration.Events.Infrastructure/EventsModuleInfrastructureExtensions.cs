using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Application.UseCases;
using EventRegistration.Events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// Events モジュールの Infrastructure サービスを DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class EventsModuleInfrastructureExtensions
{
    /// <summary>
    /// Events モジュールの DbContext、リポジトリ、ユースケースを登録する。
    /// </summary>
    public static IServiceCollection AddEventsModuleInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "Events"));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<CreateEventUseCase>();
        services.AddScoped<GetEventByIdUseCase>();
        services.AddScoped<GetAllEventsUseCase>();

        return services;
    }
}
