using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Domain;

[TestClass]
public sealed class EventTests
{
    private static readonly DateTimeOffset ValidScheduledAt = new(2026, 6, 1, 10, 0, 0, TimeSpan.FromHours(9));
    private static readonly DateTimeOffset ValidCreatedAt = new(2026, 4, 25, 0, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void Create_WithValidInput_ReturnsEvent()
    {
        var @event = Event.Create("テストイベント", "説明文", ValidScheduledAt, 100, ValidCreatedAt);

        Assert.IsNotNull(@event);
        Assert.AreNotEqual(Guid.Empty, @event.Id);
        Assert.AreEqual("テストイベント", @event.Name);
        Assert.AreEqual("説明文", @event.Description);
        Assert.AreEqual(ValidScheduledAt, @event.ScheduledAt);
        Assert.AreEqual(100, @event.Capacity);
        Assert.AreEqual(ValidCreatedAt, @event.CreatedAt);
    }

    [TestMethod]
    public void Create_WithNullDescription_Succeeds()
    {
        var @event = Event.Create("イベント", null, ValidScheduledAt, 50, ValidCreatedAt);

        Assert.IsNull(@event.Description);
    }

    [TestMethod]
    public void Create_WithMinimumCapacity_Succeeds()
    {
        var @event = Event.Create("イベント", null, ValidScheduledAt, 1, ValidCreatedAt);

        Assert.AreEqual(1, @event.Capacity);
    }

    [TestMethod]
    public void Create_GeneratesUniqueIds()
    {
        var event1 = Event.Create("イベント1", null, ValidScheduledAt, 10, ValidCreatedAt);
        var event2 = Event.Create("イベント2", null, ValidScheduledAt, 10, ValidCreatedAt);

        Assert.AreNotEqual(event1.Id, event2.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        Event.Create(null!, null, ValidScheduledAt, 10, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        Event.Create("", null, ValidScheduledAt, 10, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithWhitespaceName_ThrowsArgumentException()
    {
        Event.Create("   ", null, ValidScheduledAt, 10, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        Event.Create("イベント", null, ValidScheduledAt, 0, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Event.Create("イベント", null, ValidScheduledAt, -1, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithDefaultScheduledAt_ThrowsArgumentException()
    {
        Event.Create("イベント", null, default, 10, ValidCreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithDefaultCreatedAt_ThrowsArgumentException()
    {
        Event.Create("イベント", null, ValidScheduledAt, 10, default);
    }
}
