using EventRegistration.SharedKernel.Application.Navigation;
using Microsoft.AspNetCore.Components.Routing;

namespace EventRegistration.Web.Shell.Navigation;

/// <summary>
/// <see cref="NavigationMatch"/> と Blazor の <see cref="NavLinkMatch"/> を相互変換する拡張。
/// </summary>
public static class NavigationMatchExtensions
{
    /// <summary>
    /// <see cref="NavigationMatch"/> を <see cref="NavLinkMatch"/> に変換する。
    /// </summary>
    public static NavLinkMatch ToNavLinkMatch(NavigationMatch match) => match switch
    {
        NavigationMatch.All => NavLinkMatch.All,
        NavigationMatch.Prefix => NavLinkMatch.Prefix,
        _ => NavLinkMatch.Prefix,
    };
}
