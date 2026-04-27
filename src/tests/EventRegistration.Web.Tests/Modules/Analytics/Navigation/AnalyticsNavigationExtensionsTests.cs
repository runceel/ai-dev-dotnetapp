using EventRegistration.Analytics.Application.Navigation;
using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Analytics.Navigation;

[TestClass]
public sealed class AnalyticsNavigationExtensionsTests
{
    [TestMethod]
    public void AddAnalyticsModuleNavigation_RegistersSingletonINavigationItem()
    {
        var services = new ServiceCollection();

        services.AddAnalyticsModuleNavigation();

        var descriptor = services.Single(d => d.ServiceType == typeof(INavigationItem));
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddAnalyticsModuleNavigation_ResolvedItem_HasExpectedValues()
    {
        var services = new ServiceCollection();
        services.AddAnalyticsModuleNavigation();
        var provider = services.BuildServiceProvider();

        var item = provider.GetServices<INavigationItem>().Single();

        Assert.AreEqual("統計・分析", item.Title);
        Assert.AreEqual("/analytics", item.Href);
        Assert.AreEqual("Analytics", item.Icon);
        Assert.AreEqual("管理", item.Group);
        Assert.AreEqual(300, item.Order);
        Assert.AreEqual(NavigationMatch.Prefix, item.Match);
    }

    [TestMethod]
    public void AddAnalyticsModuleNavigation_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddAnalyticsModuleNavigation();

        Assert.AreSame(services, result);
    }
}
