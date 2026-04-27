using EventRegistration.Analytics.Domain;

namespace EventRegistration.Web.Tests.Modules.Analytics.Domain;

[TestClass]
public sealed class EventStatisticsTests
{
    [TestMethod]
    public void TotalRegistrations_IsConfirmedPlusWaitListed()
    {
        var stats = new EventStatistics(Guid.NewGuid(), 3, 2, 1, 1);
        Assert.AreEqual(5, stats.TotalRegistrations);
    }

    [TestMethod]
    public void FinalConfirmedCount_AddsPromotionsAndSubtractsCancellations()
    {
        var stats = new EventStatistics(Guid.NewGuid(), 5, 3, 2, 1);
        // 5 + 1 - 2 = 4
        Assert.AreEqual(4, stats.FinalConfirmedCount);
    }

    [TestMethod]
    public void FinalConfirmedCount_ClampsAtZero()
    {
        var stats = new EventStatistics(Guid.NewGuid(), 1, 5, 10, 0);
        Assert.AreEqual(0, stats.FinalConfirmedCount);
    }

    [TestMethod]
    public void ParticipationRate_IsZero_WhenNoRegistrations()
    {
        var stats = new EventStatistics(Guid.NewGuid(), 0, 0, 0, 0);
        Assert.AreEqual(0d, stats.ParticipationRate);
        Assert.AreEqual(0d, stats.CancellationRate);
    }

    [TestMethod]
    public void Rates_ComputedAgainstTotalRegistrations()
    {
        var stats = new EventStatistics(Guid.NewGuid(), 8, 2, 4, 0);
        // Total = 10, Final = 8 - 4 = 4
        Assert.AreEqual(0.4d, stats.ParticipationRate, 0.0001);
        Assert.AreEqual(0.4d, stats.CancellationRate, 0.0001);
    }
}

[TestClass]
public sealed class RegistrationActivityTests
{
    [TestMethod]
    public void Create_AssignsAllFields()
    {
        var eventId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var activity = RegistrationActivity.Create(
            eventId, registrationId, RegistrationActivityType.Confirmed, occurredAt);

        Assert.AreNotEqual(Guid.Empty, activity.Id);
        Assert.AreEqual(eventId, activity.EventId);
        Assert.AreEqual(registrationId, activity.RegistrationId);
        Assert.AreEqual(RegistrationActivityType.Confirmed, activity.ActivityType);
        Assert.AreEqual(occurredAt, activity.OccurredAt);
    }
}
