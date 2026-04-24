using EventRegistration.Events.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// Events モジュールの Infrastructure 層サービスを DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class EventsInfrastructureExtensions
{
    /// <summary>
    /// Events モジュールの永続化サービスを登録する。
    /// </summary>
    public static IServiceCollection AddEventsInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseInMemoryDatabase("Events"));

        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
