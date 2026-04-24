using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Infrastructure;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace EventRegistration.Web.Tests.Modules.Registrations.Infrastructure;

[TestClass]
public sealed class RegistrationsModuleInfrastructureTests
{
    [TestMethod]
    public void AddRegistrationsModuleInfrastructure_RegistersDbContext()
    {
        var services = new ServiceCollection();

        services.AddRegistrationsModuleInfrastructure();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RegistrationsDbContext));
        Assert.IsNotNull(descriptor);
    }

    [TestMethod]
    public void AddRegistrationsModuleInfrastructure_RegistersRepository()
    {
        var services = new ServiceCollection();

        services.AddRegistrationsModuleInfrastructure();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRegistrationRepository));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddRegistrationsModuleInfrastructure_RegistersUseCases()
    {
        var services = new ServiceCollection();

        services.AddRegistrationsModuleInfrastructure();

        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(RegisterParticipantUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(CancelRegistrationUseCase)));
        Assert.IsNotNull(services.FirstOrDefault(d => d.ServiceType == typeof(GetRegistrationsByEventUseCase)));
    }

    [TestMethod]
    public void AddRegistrationsModuleInfrastructure_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddRegistrationsModuleInfrastructure();

        Assert.AreSame(services, result);
    }
}
