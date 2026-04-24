using MudBlazor;

namespace EventRegistration.Web.Shell.Navigation;

/// <summary>
/// 文字列キーで指定されたアイコンを MudBlazor のアイコン定数にマッピングする。
/// 未登録のキーが指定された場合は <see cref="Icons.Material.Filled.Help"/> を返す。
/// </summary>
public static class IconResolver
{
    private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Home"] = Icons.Material.Filled.Home,
        ["Event"] = Icons.Material.Filled.Event,
        ["HowToReg"] = Icons.Material.Filled.HowToReg,
        ["People"] = Icons.Material.Filled.People,
        ["Settings"] = Icons.Material.Filled.Settings,
        ["Dashboard"] = Icons.Material.Filled.Dashboard,
        ["Analytics"] = Icons.Material.Filled.Analytics,
        ["Notifications"] = Icons.Material.Filled.Notifications,
        ["Security"] = Icons.Material.Filled.Security,
        ["Help"] = Icons.Material.Filled.Help,
    };

    /// <summary>
    /// 指定したアイコンキーに対応する MudBlazor アイコンを返す。
    /// </summary>
    public static string Resolve(string iconKey)
    {
        return IconMap.TryGetValue(iconKey, out var icon) ? icon : Icons.Material.Filled.Help;
    }
}
