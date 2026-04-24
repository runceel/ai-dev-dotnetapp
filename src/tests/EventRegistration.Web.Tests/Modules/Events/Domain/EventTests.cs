using EventRegistration.Events.Domain;

namespace EventRegistration.Web.Tests.Modules.Events.Domain;

[TestClass]
public sealed class EventTests
{
    [TestMethod]
    public void Create_ValidInputs_ReturnsEvent()
    {
        var ev = Event.Create("テストイベント", "説明", DateTimeOffset.UtcNow.AddDays(7), 10);

        Assert.IsNotNull(ev);
        Assert.AreNotEqual(Guid.Empty, ev.Id);
        Assert.AreEqual("テストイベント", ev.Name);
        Assert.AreEqual("説明", ev.Description);
        Assert.AreEqual(10, ev.Capacity);
    }

    [TestMethod]
    public void Create_NullName_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => Event.Create(null!, null, DateTimeOffset.UtcNow, 10));
    }

    [TestMethod]
    public void Create_WhitespaceName_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(
            () => Event.Create("  ", null, DateTimeOffset.UtcNow, 10));
    }

    [TestMethod]
    public void Create_ZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => Event.Create("テスト", null, DateTimeOffset.UtcNow, 0));
    }

    [TestMethod]
    public void Create_NegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => Event.Create("テスト", null, DateTimeOffset.UtcNow, -1));
    }

    [TestMethod]
    public void Create_NullDescription_SetsDescriptionToNull()
    {
        var ev = Event.Create("テスト", null, DateTimeOffset.UtcNow, 5);

        Assert.IsNull(ev.Description);
    }
}
