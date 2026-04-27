using EventRegistration.Analytics.Domain;

namespace EventRegistration.Web.Tests.Modules.Analytics.Domain;

[TestClass]
public sealed class EventStatisticsTests
{
    [TestMethod]
    public void ParticipationRate_DividesConfirmedByCapacity()
    {
        var stats = new EventStatistics(
            EventId: Guid.NewGuid(),
            EventName: "Test",
            ScheduledAt: DateTimeOffset.UtcNow,
            Capacity: 10,
            ConfirmedCount: 4,
            WaitListedCount: 2,
            CancelledCount: 1);

        Assert.AreEqual(0.4d, stats.ParticipationRate, 1e-9);
    }

    [TestMethod]
    public void ParticipationRate_CapacityZero_ReturnsZero()
    {
        var stats = new EventStatistics(Guid.NewGuid(), "x", DateTimeOffset.UtcNow, 0, 5, 0, 0);
        Assert.AreEqual(0d, stats.ParticipationRate);
    }

    [TestMethod]
    public void CancellationRate_DividesByTotal()
    {
        var stats = new EventStatistics(Guid.NewGuid(), "x", DateTimeOffset.UtcNow, 10, 4, 2, 4);
        Assert.AreEqual(0.4d, stats.CancellationRate, 1e-9);
    }

    [TestMethod]
    public void CancellationRate_NoRegistrations_ReturnsZero()
    {
        var stats = new EventStatistics(Guid.NewGuid(), "x", DateTimeOffset.UtcNow, 10, 0, 0, 0);
        Assert.AreEqual(0d, stats.CancellationRate);
    }
}

[TestClass]
public sealed class OverallSummaryTests
{
    [TestMethod]
    public void OverallCancellationRate_DividesCancelledByTotal()
    {
        var summary = new OverallSummary(
            TotalEvents: 3,
            TotalRegistrations: 10,
            TotalConfirmed: 5,
            TotalWaitListed: 3,
            TotalCancelled: 2);

        Assert.AreEqual(0.2d, summary.OverallCancellationRate, 1e-9);
    }

    [TestMethod]
    public void OverallCancellationRate_NoRegistrations_ReturnsZero()
    {
        var summary = new OverallSummary(0, 0, 0, 0, 0);
        Assert.AreEqual(0d, summary.OverallCancellationRate);
    }
}
