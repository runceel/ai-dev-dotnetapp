using EventRegistration.SharedKernel.Application.DemoData;
using EventRegistration.Web.DemoData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EventRegistration.Web.Tests.DemoData;

[TestClass]
public sealed class DemoDataHostedServiceTests
{
    private sealed class RecordingSeeder(int order, List<int> log) : IDemoDataSeeder
    {
        public int Order => order;
        public Task SeedAsync(CancellationToken cancellationToken)
        {
            log.Add(order);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingSeeder(int order) : IDemoDataSeeder
    {
        public int Order => order;
        public Task SeedAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("boom");
    }

    private static DemoDataHostedService BuildService(IServiceProvider provider, bool enabled)
    {
        var options = Options.Create(new DemoDataOptions { Enabled = enabled });
        return new DemoDataHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<DemoDataHostedService>.Instance);
    }

    [TestMethod]
    public async Task StartAsync_DoesNotInvokeSeeders_WhenDisabled()
    {
        var log = new List<int>();
        var services = new ServiceCollection();
        services.AddScoped<IDemoDataSeeder>(_ => new RecordingSeeder(10, log));
        var provider = services.BuildServiceProvider();

        var sut = BuildService(provider, enabled: false);
        await sut.StartAsync(CancellationToken.None);

        Assert.AreEqual(0, log.Count);
    }

    [TestMethod]
    public async Task StartAsync_RunsSeedersInOrderAscending()
    {
        var log = new List<int>();
        var services = new ServiceCollection();
        services.AddScoped<IDemoDataSeeder>(_ => new RecordingSeeder(20, log));
        services.AddScoped<IDemoDataSeeder>(_ => new RecordingSeeder(10, log));
        services.AddScoped<IDemoDataSeeder>(_ => new RecordingSeeder(30, log));
        var provider = services.BuildServiceProvider();

        var sut = BuildService(provider, enabled: true);
        await sut.StartAsync(CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, log);
    }

    [TestMethod]
    public async Task StartAsync_DoesNotPropagate_SeederException()
    {
        var log = new List<int>();
        var services = new ServiceCollection();
        services.AddScoped<IDemoDataSeeder>(_ => new ThrowingSeeder(10));
        services.AddScoped<IDemoDataSeeder>(_ => new RecordingSeeder(20, log));
        var provider = services.BuildServiceProvider();

        var sut = BuildService(provider, enabled: true);
        await sut.StartAsync(CancellationToken.None);

        // Throwing seeder must not abort the run; subsequent seeders still execute.
        CollectionAssert.AreEqual(new[] { 20 }, log);
    }
}
