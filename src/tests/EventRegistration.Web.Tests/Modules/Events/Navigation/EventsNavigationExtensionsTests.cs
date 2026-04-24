using EventRegistration.Events.Application.Navigation;
using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Events.Navigation;

[TestClass]
public sealed class EventsNavigationExtensionsTests
{
    [TestMethod]
    public void AddEventsModuleNavigation_RegistersSingletonINavigationItem()
    {
        var services = new ServiceCollection();

        services.AddEventsModuleNavigation();

        var descriptor = services.Single(d => d.ServiceType == typeof(INavigationItem));
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddEventsModuleNavigation_ResolvedItem_HasExpectedValues()
    {
        var services = new ServiceCollection();
        services.AddEventsModuleNavigation();
        var provider = services.BuildServiceProvider();

        var item = provider.GetServices<INavigationItem>().Single();

        Assert.AreEqual("イベント管理", item.Title);
        Assert.AreEqual("Event", item.Icon);
        Assert.AreEqual("イベント", item.Group);
        Assert.AreEqual(100, item.Order);
        Assert.AreEqual(NavigationMatch.Prefix, item.Match);
    }

    [TestMethod]
    public void AddEventsModuleNavigation_ResolveTwice_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        services.AddEventsModuleNavigation();
        var provider = services.BuildServiceProvider();

        var first = provider.GetServices<INavigationItem>().Single();
        var second = provider.GetServices<INavigationItem>().Single();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void AddEventsModuleNavigation_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddEventsModuleNavigation();

        Assert.AreSame(services, result);
    }
}
