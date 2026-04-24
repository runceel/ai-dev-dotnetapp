namespace EventRegistration.SharedKernel.Application.Navigation;

/// <summary>
/// <see cref="INavigationItem"/> の既定実装。
/// </summary>
public sealed record NavigationItem(
    string Title,
    string Href,
    string Icon,
    string Group,
    int Order,
    NavigationMatch Match) : INavigationItem;
