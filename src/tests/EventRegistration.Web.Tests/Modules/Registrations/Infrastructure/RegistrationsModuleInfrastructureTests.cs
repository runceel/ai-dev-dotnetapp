using EventRegistration.Registrations.Application.Repositories;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    [TestMethod]
    public void RegistrationsDbContext_HasUniqueFilteredIndexOnEventIdAndEmail()
    {
        var options = new DbContextOptionsBuilder<RegistrationsDbContext>()
            .UseInMemoryDatabase(databaseName: $"index-test-{Guid.NewGuid()}")
            .Options;

        using var context = new RegistrationsDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Registration));
        Assert.IsNotNull(entityType);

        var index = entityType.GetIndexes().SingleOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == nameof(Registration.EventId)) &&
            i.Properties.Any(p => p.Name == nameof(Registration.Email)));

        Assert.IsNotNull(index, "EventId + Email の複合インデックスが定義されていること");
        Assert.IsTrue(index.IsUnique, "インデックスは IsUnique であること");
        Assert.AreEqual("[Status] <> 2", index.GetFilter(), "Cancelled を除外するフィルターが設定されていること");
    }
}
