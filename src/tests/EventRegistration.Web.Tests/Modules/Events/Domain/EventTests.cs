using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Domain;

[TestClass]
public sealed class EventTests
{
    [TestMethod]
    public void Create_WithValidInput_ReturnsEvent()
    {
        var scheduledAt = DateTimeOffset.UtcNow.AddDays(7);

        var ev = Event.Create("テストイベント", "説明文", scheduledAt, 50);

        Assert.AreNotEqual(Guid.Empty, ev.Id);
        Assert.AreEqual("テストイベント", ev.Name);
        Assert.AreEqual("説明文", ev.Description);
        Assert.AreEqual(scheduledAt, ev.ScheduledAt);
        Assert.AreEqual(50, ev.Capacity);
        Assert.IsTrue(ev.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [TestMethod]
    public void Create_WithNullDescription_SetsDescriptionToNull()
    {
        var ev = Event.Create("イベント", null, DateTimeOffset.UtcNow.AddDays(1), 10);

        Assert.IsNull(ev.Description);
    }

    [TestMethod]
    public void Create_WithWhitespaceDescription_NormalizesToNull()
    {
        var ev = Event.Create("イベント", "   ", DateTimeOffset.UtcNow.AddDays(1), 10);

        Assert.IsNull(ev.Description);
    }

    [TestMethod]
    public void Create_TrimsName()
    {
        var ev = Event.Create("  テストイベント  ", null, DateTimeOffset.UtcNow.AddDays(1), 10);

        Assert.AreEqual("テストイベント", ev.Name);
    }

    [TestMethod]
    public void Create_TrimsDescription()
    {
        var ev = Event.Create("イベント", "  説明  ", DateTimeOffset.UtcNow.AddDays(1), 10);

        Assert.AreEqual("説明", ev.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        Event.Create("", null, DateTimeOffset.UtcNow.AddDays(1), 10);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithWhitespaceName_ThrowsArgumentException()
    {
        Event.Create("   ", null, DateTimeOffset.UtcNow.AddDays(1), 10);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        Event.Create("イベント", null, DateTimeOffset.UtcNow.AddDays(1), 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Event.Create("イベント", null, DateTimeOffset.UtcNow.AddDays(1), -1);
    }

    [TestMethod]
    public void Create_WithCapacityOne_Succeeds()
    {
        var ev = Event.Create("イベント", null, DateTimeOffset.UtcNow.AddDays(1), 1);

        Assert.AreEqual(1, ev.Capacity);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithDefaultScheduledAt_ThrowsArgumentException()
    {
        Event.Create("イベント", null, default, 10);
    }

    [TestMethod]
    public void Create_GeneratesUniqueIds()
    {
        var ev1 = Event.Create("イベント1", null, DateTimeOffset.UtcNow.AddDays(1), 10);
        var ev2 = Event.Create("イベント2", null, DateTimeOffset.UtcNow.AddDays(2), 20);

        Assert.AreNotEqual(ev1.Id, ev2.Id);
    }
}
