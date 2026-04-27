using EventRegistration.Registrations.Application.Navigation;
using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Registrations.Navigation;

[TestClass]
public sealed class RegistrationsNavigationExtensionsTests
{
    [TestMethod]
    public void AddRegistrationsModuleNavigation_DoesNotRegisterINavigationItem()
    {
        var services = new ServiceCollection();

        services.AddRegistrationsModuleNavigation();

        var descriptors = services.Where(d => d.ServiceType == typeof(INavigationItem)).ToList();
        Assert.AreEqual(0, descriptors.Count);
    }

    [TestMethod]
    public void AddRegistrationsModuleNavigation_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddRegistrationsModuleNavigation();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddBothModules_OnlyEventsItemRegistered()
    {
        var services = new ServiceCollection();
        services.AddRegistrationsModuleNavigation();
        services.AddEventsModuleNavigation_Local();

        var provider = services.BuildServiceProvider();
        var items = provider.GetServices<INavigationItem>().ToList();

        Assert.AreEqual(1, items.Count);
        Assert.IsTrue(items.Any(i => i.Title == "イベント管理"));
        Assert.IsFalse(items.Any(i => i.Title == "参加登録"));
    }
}

internal static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddEventsModuleNavigation_Local(this IServiceCollection services)
        => EventRegistration.Events.Application.Navigation.EventsNavigationExtensions.AddEventsModuleNavigation(services);
}
