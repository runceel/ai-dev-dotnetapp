using EventRegistration.SharedKernel.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventRegistration.SharedKernel.Infrastructure.Events;

/// <summary>
/// SharedKernel が提供するイベント基盤を DI コンテナへ登録する拡張メソッド。
/// </summary>
public static class SharedKernelEventsServiceCollectionExtensions
{
    /// <summary>
    /// 既定の <see cref="IDomainEventDispatcher"/> 実装を登録する。
    /// 複数回呼び出しても重複登録されない。
    /// </summary>
    public static IServiceCollection AddSharedKernelDomainEvents(this IServiceCollection services)
    {
        services.TryAddScoped<IDomainEventDispatcher, ServiceProviderDomainEventDispatcher>();
        return services;
    }
}
