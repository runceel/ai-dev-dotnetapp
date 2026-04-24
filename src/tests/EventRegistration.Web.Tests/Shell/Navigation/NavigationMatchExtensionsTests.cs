using EventRegistration.SharedKernel.Application.Navigation;
using EventRegistration.Web.Shell.Navigation;
using Microsoft.AspNetCore.Components.Routing;

namespace EventRegistration.Web.Tests.Shell.Navigation;

[TestClass]
public sealed class NavigationMatchExtensionsTests
{
    [TestMethod]
    public void ToNavLinkMatch_All_ReturnsNavLinkMatchAll()
    {
        var actual = NavigationMatchExtensions.ToNavLinkMatch(NavigationMatch.All);

        Assert.AreEqual(NavLinkMatch.All, actual);
    }

    [TestMethod]
    public void ToNavLinkMatch_Prefix_ReturnsNavLinkMatchPrefix()
    {
        var actual = NavigationMatchExtensions.ToNavLinkMatch(NavigationMatch.Prefix);

        Assert.AreEqual(NavLinkMatch.Prefix, actual);
    }

    [TestMethod]
    public void ToNavLinkMatch_UndefinedValue_DefaultsToPrefix()
    {
        var undefined = (NavigationMatch)999;

        var actual = NavigationMatchExtensions.ToNavLinkMatch(undefined);

        Assert.AreEqual(NavLinkMatch.Prefix, actual);
    }
}
