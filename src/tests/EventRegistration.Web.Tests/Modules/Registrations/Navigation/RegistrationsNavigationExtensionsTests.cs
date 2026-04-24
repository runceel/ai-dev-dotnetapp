using EventRegistration.Registrations.Application.Navigation;
using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Registrations.Navigation;

[TestClass]
public sealed class RegistrationsNavigationExtensionsTests
{
    [TestMethod]
    public void AddRegistrationsModuleNavigation_RegistersSingletonINavigationItem()
    {
        var services = new ServiceCollection();

        services.AddRegistrationsModuleNavigation();

        var descriptor = services.Single(d => d.ServiceType == typeof(INavigationItem));
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddRegistrationsModuleNavigation_ResolvedItem_HasExpectedValues()
    {
        var services = new ServiceCollection();
        services.AddRegistrationsModuleNavigation();
        var provider = services.BuildServiceProvider();

        var item = provider.GetServices<INavigationItem>().Single();

        Assert.AreEqual("参加登録", item.Title);
        Assert.AreEqual("HowToReg", item.Icon);
        Assert.AreEqual("参加者", item.Group);
        Assert.AreEqual(200, item.Order);
        Assert.AreEqual(NavigationMatch.Prefix, item.Match);
    }

    [TestMethod]
    public void AddRegistrationsModuleNavigation_ResolveTwice_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        services.AddRegistrationsModuleNavigation();
        var provider = services.BuildServiceProvider();

        var first = provider.GetServices<INavigationItem>().Single();
        var second = provider.GetServices<INavigationItem>().Single();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void AddRegistrationsModuleNavigation_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddRegistrationsModuleNavigation();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddBothModules_BothItemsRegistered()
    {
        var services = new ServiceCollection();
        services.AddRegistrationsModuleNavigation();
        services.AddEventsModuleNavigation_Local();

        var provider = services.BuildServiceProvider();
        var items = provider.GetServices<INavigationItem>().ToList();

        Assert.AreEqual(2, items.Count);
        Assert.IsTrue(items.Any(i => i.Title == "参加登録"));
        Assert.IsTrue(items.Any(i => i.Title == "イベント管理"));
    }
}

internal static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddEventsModuleNavigation_Local(this IServiceCollection services)
        => EventRegistration.Events.Application.Navigation.EventsNavigationExtensions.AddEventsModuleNavigation(services);
}
