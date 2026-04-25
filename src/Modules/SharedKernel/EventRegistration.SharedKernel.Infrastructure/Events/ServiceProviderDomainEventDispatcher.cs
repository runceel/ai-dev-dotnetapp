using System.Collections.Concurrent;
using System.Reflection;
using EventRegistration.SharedKernel.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventRegistration.SharedKernel.Infrastructure.Events;

/// <summary>
/// <see cref="IServiceProvider"/> から <see cref="IDomainEventHandler{TEvent}"/> を解決して
/// 同期的に呼び出す既定のディスパッチャ実装。
/// </summary>
/// <remarks>
/// - 各ハンドラは順次呼び出される（並列化しない）。<br/>
/// - 個々のハンドラ内で発生した例外はログに記録され、呼び出し元へは伝播しない。
///   これによりユースケースの主処理が副作用の失敗で巻き戻ることを防ぐ（AC-04 順守）。
/// </remarks>
public sealed class ServiceProviderDomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceProviderDomainEventDispatcher> _logger;

    public ServiceProviderDomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<ServiceProviderDomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var handlerType = HandlerTypeCache.GetOrAdd(
            eventType,
            static t => typeof(IDomainEventHandler<>).MakeGenericType(t));
        var handleMethod = HandleMethodCache.GetOrAdd(
            handlerType,
            static h => h.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!);

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            try
            {
                var task = (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Domain event handler {HandlerType} failed while handling {EventType}.",
                    handler.GetType().FullName,
                    eventType.FullName);
            }
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
