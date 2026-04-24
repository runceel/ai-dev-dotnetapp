using EventRegistration.SharedKernel.Application.Navigation;

namespace EventRegistration.Web.Tests.Modules.SharedKernel.Navigation;

[TestClass]
public sealed class NavigationItemTests
{
    [TestMethod]
    public void Constructor_SetsAllProperties()
    {
        var item = new NavigationItem(
            Title: "イベント",
            Href: "/events",
            Icon: "Event",
            Group: "管理",
            Order: 100,
            Match: NavigationMatch.Prefix);

        Assert.AreEqual("イベント", item.Title);
        Assert.AreEqual("/events", item.Href);
        Assert.AreEqual("Event", item.Icon);
        Assert.AreEqual("管理", item.Group);
        Assert.AreEqual(100, item.Order);
        Assert.AreEqual(NavigationMatch.Prefix, item.Match);
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        var a = new NavigationItem("A", "/a", "Home", "G", 1, NavigationMatch.All);
        var b = new NavigationItem("A", "/a", "Home", "G", 1, NavigationMatch.All);

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new NavigationItem("A", "/a", "Home", "G", 1, NavigationMatch.All);
        var b = new NavigationItem("B", "/a", "Home", "G", 1, NavigationMatch.All);

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Implements_INavigationItem()
    {
        INavigationItem item = new NavigationItem("T", "/t", "Help", "G", 0, NavigationMatch.Prefix);

        Assert.AreEqual("T", item.Title);
        Assert.AreEqual("/t", item.Href);
    }
}
