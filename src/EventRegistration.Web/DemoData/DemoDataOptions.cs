namespace EventRegistration.Web.DemoData;

/// <summary>
/// デモ用シードデータ投入機能の設定。
/// </summary>
public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";

    /// <summary>
    /// 起動時にデモ用データを投入するかどうか。
    /// 設定が省略された場合、Development 環境では既定で <c>true</c>、それ以外は <c>false</c>。
    /// </summary>
    public bool Enabled { get; set; }
}
