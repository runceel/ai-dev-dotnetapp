using EventRegistration.SharedKernel.Application.Events;
using EventRegistration.SharedKernel.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.SharedKernel.Events;

[TestClass]
public sealed class ServiceProviderDomainEventDispatcherTests
{
    [TestMethod]
    public async Task DispatchAsync_NoHandlerRegistered_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSharedKernelDomainEvents();
        await using var sp = services.BuildServiceProvider();

        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();
        var domainEvent = new TestEvent(DateTimeOffset.UtcNow);

        // ハンドラ未登録でも例外を投げない（AC-04）
        await dispatcher.DispatchAsync(domainEvent);
    }

    [TestMethod]
    public async Task DispatchAsync_InvokesAllRegisteredHandlers()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSharedKernelDomainEvents();
        services.AddSingleton<IDomainEventHandler<TestEvent>, RecordingHandler>();
        services.AddSingleton<IDomainEventHandler<TestEvent>, RecordingHandler>();
        await using var sp = services.BuildServiceProvider();

        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();
        var handlers = sp.GetServices<IDomainEventHandler<TestEvent>>().Cast<RecordingHandler>().ToList();

        await dispatcher.DispatchAsync(new TestEvent(DateTimeOffset.UtcNow));

        Assert.AreEqual(2, handlers.Count);
        Assert.IsTrue(handlers.All(h => h.Called));
    }

    [TestMethod]
    public async Task DispatchAsync_HandlerThrows_OtherHandlersStillRunAndExceptionSwallowed()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSharedKernelDomainEvents();
        services.AddSingleton<IDomainEventHandler<TestEvent>, ThrowingHandler>();
        services.AddSingleton<IDomainEventHandler<TestEvent>, RecordingHandler>();
        await using var sp = services.BuildServiceProvider();

        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();
        var recording = sp.GetServices<IDomainEventHandler<TestEvent>>()
            .OfType<RecordingHandler>()
            .Single();

        // 例外は呼び出し元へ伝播せず、後続ハンドラも実行される
        await dispatcher.DispatchAsync(new TestEvent(DateTimeOffset.UtcNow));

        Assert.IsTrue(recording.Called);
    }

    private sealed record TestEvent(DateTimeOffset OccurredAt) : IDomainEvent;

    private sealed class RecordingHandler : IDomainEventHandler<TestEvent>
    {
        public bool Called { get; private set; }
        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IDomainEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }
}
