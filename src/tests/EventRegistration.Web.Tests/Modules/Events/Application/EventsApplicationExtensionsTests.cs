using EventRegistration.Events.Application;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Events.Application;

[TestClass]
public sealed class EventsApplicationExtensionsTests
{
    [TestMethod]
    public void AddEventsApplication_RegistersIEventsAppService()
    {
        var services = new ServiceCollection();

        services.AddEventsApplication();

        var descriptor = services.Single(d => d.ServiceType == typeof(IEventsAppService));
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddEventsApplication_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddEventsApplication();

        Assert.AreSame(services, result);
    }
}
