using EventRegistration.Web.Shell.Navigation;
using MudBlazor;

namespace EventRegistration.Web.Tests.Shell.Navigation;

[TestClass]
public sealed class IconResolverTests
{
    [TestMethod]
    public void Resolve_KnownKeyHome_ReturnsHomeIcon()
        => Assert.AreEqual(Icons.Material.Filled.Home, IconResolver.Resolve("Home"));

    [TestMethod]
    public void Resolve_KnownKeyEvent_ReturnsEventIcon()
        => Assert.AreEqual(Icons.Material.Filled.Event, IconResolver.Resolve("Event"));

    [TestMethod]
    public void Resolve_KnownKeyHowToReg_ReturnsHowToRegIcon()
        => Assert.AreEqual(Icons.Material.Filled.HowToReg, IconResolver.Resolve("HowToReg"));

    [TestMethod]
    public void Resolve_KnownKeyPeople_ReturnsPeopleIcon()
        => Assert.AreEqual(Icons.Material.Filled.People, IconResolver.Resolve("People"));

    [TestMethod]
    public void Resolve_KnownKeySettings_ReturnsSettingsIcon()
        => Assert.AreEqual(Icons.Material.Filled.Settings, IconResolver.Resolve("Settings"));

    [TestMethod]
    public void Resolve_KnownKeyDashboard_ReturnsDashboardIcon()
        => Assert.AreEqual(Icons.Material.Filled.Dashboard, IconResolver.Resolve("Dashboard"));

    [TestMethod]
    public void Resolve_KnownKeyAnalytics_ReturnsAnalyticsIcon()
        => Assert.AreEqual(Icons.Material.Filled.Analytics, IconResolver.Resolve("Analytics"));

    [TestMethod]
    public void Resolve_KnownKeyNotifications_ReturnsNotificationsIcon()
        => Assert.AreEqual(Icons.Material.Filled.Notifications, IconResolver.Resolve("Notifications"));

    [TestMethod]
    public void Resolve_KnownKeySecurity_ReturnsSecurityIcon()
        => Assert.AreEqual(Icons.Material.Filled.Security, IconResolver.Resolve("Security"));

    [TestMethod]
    public void Resolve_KnownKeyHelp_ReturnsHelpIcon()
        => Assert.AreEqual(Icons.Material.Filled.Help, IconResolver.Resolve("Help"));

    [TestMethod]
    [DataRow("UnknownIcon")]
    [DataRow("")]
    [DataRow("not-a-real-key")]
    public void Resolve_UnknownKey_FallsBackToHelp(string key)
    {
        var actual = IconResolver.Resolve(key);

        Assert.AreEqual(Icons.Material.Filled.Help, actual);
    }

    [TestMethod]
    [DataRow("event")]
    [DataRow("EVENT")]
    [DataRow("Event")]
    [DataRow("eVeNt")]
    public void Resolve_IsCaseInsensitive(string key)
    {
        var actual = IconResolver.Resolve(key);

        Assert.AreEqual(Icons.Material.Filled.Event, actual);
    }
}
