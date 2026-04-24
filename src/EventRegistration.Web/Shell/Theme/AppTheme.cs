using MudBlazor;

namespace EventRegistration.Web.Shell.Theme;

/// <summary>
/// アプリケーション共通の MudBlazor テーマ定義。
/// </summary>
public static class AppTheme
{
    /// <summary>
    /// ライトテーマ (.NET ブランドカラーをベースとしたカスタムパレット)。
    /// </summary>
    public static readonly MudTheme Light = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#512BD4",        // .NET Purple
            Secondary = "#68217A",      // Visual Studio Purple
            Tertiary = "#9B4DCA",
            AppbarBackground = "#512BD4",
            DrawerBackground = "#F5F5F5",
            DrawerText = "#424242",
            Background = "#FFFFFF",
            Surface = "#FFFFFF",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"]
            }
        }
    };
}
