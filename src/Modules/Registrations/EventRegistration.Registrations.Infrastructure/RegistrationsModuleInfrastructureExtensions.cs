using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Registrations.Infrastructure;

/// <summary>
/// Registrations モジュールの Infrastructure サービスを DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class RegistrationsModuleInfrastructureExtensions
{
    /// <summary>
    /// Registrations モジュールの DbContext、リポジトリ、ユースケースを登録する。
    /// </summary>
    public static IServiceCollection AddRegistrationsModuleInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<RegistrationsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "Registrations"));

        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<RegisterParticipantUseCase>();
        services.AddScoped<CancelRegistrationUseCase>();
        services.AddScoped<GetRegistrationsByEventUseCase>();

        return services;
    }
}
