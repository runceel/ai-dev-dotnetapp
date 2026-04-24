namespace EventRegistration.SharedKernel.Application.Navigation;

/// <summary>
/// 各モジュールが提供するナビゲーション項目を表す抽象。
/// UI フレームワーク (MudBlazor 等) には依存しない。
/// </summary>
public interface INavigationItem
{
    /// <summary>表示テキスト。</summary>
    string Title { get; }

    /// <summary>遷移先パス。</summary>
    string Href { get; }

    /// <summary>アイコン名 (UI 層で解決する文字列キー)。</summary>
    string Icon { get; }

    /// <summary>所属するナビゲーショングループ名。</summary>
    string Group { get; }

    /// <summary>同一グループ内での表示順 (昇順)。</summary>
    int Order { get; }

    /// <summary>アクティブ判定ルール。</summary>
    NavigationMatch Match { get; }
}
