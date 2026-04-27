using EventRegistration.Analytics.Application.Navigation;
using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Analytics.Navigation;

[TestClass]
public sealed class AnalyticsNavigationExtensionsTests
{
    [TestMethod]
    public void AddAnalyticsModuleNavigation_RegistersAnalyticsNavItem()
    {
        var services = new ServiceCollection();
        services.AddAnalyticsModuleNavigation();

        using var provider = services.BuildServiceProvider();
        var items = provider.GetServices<INavigationItem>().ToList();

        Assert.AreEqual(1, items.Count);
        var item = items[0];
        Assert.AreEqual("統計レポート", item.Title);
        Assert.AreEqual("/analytics", item.Href);
        Assert.AreEqual("Analytics", item.Icon);
        Assert.AreEqual("分析", item.Group);
        Assert.AreEqual(NavigationMatch.Prefix, item.Match);
    }
}
