namespace EventRegistration.SharedKernel.Application.Navigation;

/// <summary>
/// ナビゲーション項目のアクティブ判定方法。
/// </summary>
public enum NavigationMatch
{
    /// <summary>
    /// 現在のパスが <see cref="INavigationItem.Href"/> で始まる場合にアクティブとみなす。
    /// </summary>
    Prefix,

    /// <summary>
    /// 現在のパスが <see cref="INavigationItem.Href"/> と完全一致する場合にアクティブとみなす。
    /// </summary>
    All,
}
