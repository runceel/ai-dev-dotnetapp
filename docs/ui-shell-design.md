# UI Shell 設計（MudBlazor 基盤 + ナビゲーション Self-Registration）

> 対象: イベント参加登録システム UI Shell 基盤（SPEC: [event-registration-system-spec.md](./event-registration-system-spec.md) / Issue: [#3](https://github.com/runceel/ai-dev-dotnetapp/issues/3) / PR: TBD）
> ステータス: **骨子作成（Phase 2 / Step 2.2.5）**。詳細は Phase 3 実装完了後（Step 3.4.5）に追記する。
> 関連: [architecture.md](./architecture.md)（Walking Skeleton 全体構造）

---

## 概要

本ドキュメントは、`EventRegistration.Web` に **MudBlazor を導入し、AppBar + 折りたたみ Drawer + メインコンテンツ領域からなる UI Shell** を構築する設計を定義する。あわせて、各業務モジュール（`Events` / `Registrations` …）が **自身のナビゲーション項目を DI 経由で自己登録**する `INavigationItem` 抽象（Self-Registration パターン / PAT-001）を導入し、Shell 側はモジュールの追加・削除に対してコード変更不要で項目を表示できるようにする。

### 主要なポイント

| 項目 | 内容 |
|------|------|
| UI フレームワーク | MudBlazor（バージョンは実装時に確定 / TODO: Phase 3 で記載） |
| Shell 構成 | `MudThemeProvider` + Provider 群 → `MudLayout`（`MudAppBar` / `MudDrawer` / `MudMainContent`） |
| ナビゲーション抽象 | `INavigationItem`（`SharedKernel.Application/Navigation`） |
| ナビゲーション登録方式 | Self-Registration（モジュール側 `AddXxxModuleNavigation()` で `INavigationItem` を Singleton 登録） |
| Shell 側解決方法 | `@inject IEnumerable<INavigationItem>` をレイアウトコンポーネントに注入 |
| テーマ | `EventRegistration.Web/Shell/Theme/AppTheme.cs`（`MudTheme Light`、Primary `#512BD4`） |
| アイコン解決 | `EventRegistration.Web/Shell/Navigation/IconResolver.cs`（文字列キー → `MudBlazor.Icons.Material.Filled.*` SVG） |
| Match 変換 | `NavigationMatchExtensions.ToNavLinkMatch()`（`NavigationMatch` → `NavLinkMatch`） |
| 依存方向 | `Web → Modules.Application → SharedKernel.Application` のみ（CON-006 厳守） |
| Composition Root | `EventRegistration.Web/Program.cs`（`AddMudServices()` と各 `AddXxxModuleNavigation()` 呼び出し） |

### 関連ドキュメント / コメント

- 仕様: [event-registration-system-spec.md](./event-registration-system-spec.md)
- 設計方針コメント（Architect / Step 2.1）: Issue #3（本 PR 関連コメント）
- 実装計画コメント（Developer / Step 2.2）: Issue #3（本 PR 関連コメント）
- レビューコメント（Reviewer / Step 1.2）: Issue #3
- 全体アーキテクチャ: [architecture.md](./architecture.md)

> 本セクションのコメント ID は Phase 3 で正式 URL（`#issuecomment-XXXX`）に置換する。

---

## 1. ディレクトリ構成（変更後の予定構成）

> 本節は **Phase 2 時点の予定構成**。`<!-- TODO: Phase 3 で実態と差分を確認し更新 -->`。

```
ai-dev-dotnetapp/
├── docs/
│   ├── architecture.md
│   ├── event-registration-system-spec.md
│   └── ui-shell-design.md                          # 本ドキュメント
└── src/
    ├── EventRegistration.Web/
    │   ├── Program.cs                              # AddMudServices() / AddXxxModuleNavigation() を追加
    │   ├── EventRegistration.Web.csproj            # MudBlazor PackageReference 追加 / SharedKernel.Application への ProjectReference 追加
    │   ├── Components/
    │   │   ├── App.razor                           # MudBlazor CSS/JS / Roboto フォント参照を追加
    │   │   └── Layout/
    │   │       └── MainLayout.razor                # MudLayout 構造へ刷新（既存 blazor-error-ui は MainContent 外に維持）
    │   └── Shell/                                  # （新規）UI Shell 専用領域
    │       ├── Theme/
    │       │   └── AppTheme.cs                     # MudTheme Light（Primary=#512BD4 ほか）
    │       └── Navigation/
    │           ├── IconResolver.cs                 # 文字列キー → MudBlazor SVG 解決（internal static）
    │           └── NavigationMatchExtensions.cs    # NavigationMatch → NavLinkMatch 変換（internal static）
    ├── Modules/
        ├── SharedKernel/
        │   └── EventRegistration.SharedKernel.Application/
        │       └── Navigation/                     # （新規）UI 中立ナビゲーション抽象
        │           ├── INavigationItem.cs
        │           ├── NavigationItem.cs           # sealed record（既定値 Order=100, Match=Prefix）
        │           └── NavigationMatch.cs          # enum { Prefix=0, All=1 }
        ├── Events/
        │   └── EventRegistration.Events.Application/
        │       └── Navigation/
        │           └── EventsModuleNavigationExtensions.cs        # AddEventsModuleNavigation()
        └── Registrations/
            └── EventRegistration.Registrations.Application/
                └── Navigation/
                    └── RegistrationsModuleNavigationExtensions.cs # AddRegistrationsModuleNavigation()
    └── tests/                                                    # （新規）テストプロジェクト集約ディレクトリ
        ├── EventRegistration.SharedKernel.Application.Tests/     # NavigationItem / NavigationMatch のユニットテスト（§7.1）
        │   └── EventRegistration.SharedKernel.Application.Tests.csproj
        ├── EventRegistration.Events.Application.Tests/           # AddEventsModuleNavigation() のテスト（§7.1）
        │   └── EventRegistration.Events.Application.Tests.csproj
        ├── EventRegistration.Registrations.Application.Tests/    # AddRegistrationsModuleNavigation() のテスト（§7.1）
        │   └── EventRegistration.Registrations.Application.Tests.csproj
        └── EventRegistration.Web.Tests/                          # IconResolver / NavigationMatchExtensions / Composition Root 統合テスト（§7.1）
            └── EventRegistration.Web.Tests.csproj
```

> 注: `src/tests/` 配下のテストプロジェクトはいずれも MSTest 4.x + FluentAssertions（`csharp-mstest` スキル準拠）で構成し、`EventRegistration.sln` に追加する。各テストプロジェクトの最終ファイル構成・実テスト数は Phase 3（Step 3.4.5）で実態を反映する（§8 TODO 参照）。

> 注: 各モジュール `Application` プロジェクトには、必要に応じて `Microsoft.Extensions.DependencyInjection.Abstractions` への PackageReference を追加する（`IServiceCollection` 拡張メソッド実装のための最小依存）。`MudBlazor` および `EventRegistration.Web` への参照は **モジュール側からは一切持たない**（CON-002 / CON-006 / AC-014）。

---

## 2. MudBlazor 導入設定

<!-- TODO: Phase 3 で確定したパッケージバージョン・実コードを反映 -->

### 2.1 NuGet パッケージ参照

- `EventRegistration.Web.csproj` に `MudBlazor` を `PackageReference` として追加する（バージョンは実装時に NuGet 最新の `net10.0` 互換版を採用予定 / Phase 3 で確定）。
- 既存の `BlazorDisableThrowNavigationException` 等の `PropertyGroup` 設定は **改変せず維持**（CON-004）。
- モジュール側プロジェクトには MudBlazor を **追加しない**。

### 2.2 `Program.cs`（Composition Root）

```csharp
// 追加行（既存の AddRazorComponents()/AddInteractiveServerComponents() 等の隣に配置）
builder.Services.AddMudServices();
builder.Services.AddEventsModuleNavigation();
builder.Services.AddRegistrationsModuleNavigation();
```

- `AddMudServices()` は `EventRegistration.Web/Program.cs` でのみ呼ぶ（CON-001 Composition Root 単一原則）。
- 各モジュールの `AddXxxModuleNavigation()` は **同期的に Singleton で `INavigationItem` を登録**する（順序非依存）。

### 2.3 `Components/App.razor`

- `<head>` に MudBlazor の CSS（`_content/MudBlazor/MudBlazor.min.css`）と Google Fonts の Roboto を追加。
- `<body>` 末尾の `<Routes />` 後に MudBlazor の JS（`_content/MudBlazor/MudBlazor.min.js`）を追加。
- 詳細なタグ位置・属性は Phase 3 で実装後に追記する。

---

## 3. UI Shell 構成（`MainLayout.razor`）

<!-- TODO: Phase 3 で確定した razor 全文を埋め込む -->

### 3.1 構造

```
MudThemeProvider (Theme=AppTheme.Light, IsDarkMode=false)
MudPopoverProvider
MudDialogProvider
MudSnackbarProvider

MudLayout
├── MudAppBar (Color=Primary)
│   ├── MudIconButton (Menu アイコン / OnClick=ToggleDrawer)
│   └── MudText (Typo=h6 / "Event Registration")
├── MudDrawer (@bind-Open=_drawerOpen / Variant=Responsive / Breakpoint=Md / ClipMode=Always)
│   └── MudNavMenu
│       ├── （Group=null の項目）→ MudNavLink を直置き
│       └── （Group!=null の項目）→ Group ごとに MudNavGroup でまとめ、配下に MudNavLink
└── MudMainContent
    └── @Body

<div id="blazor-error-ui" data-nosnippet> ... </div>   # 既存をそのまま外側に維持（NFR-003 / AC-012）
```

### 3.2 動作仕様

| 項目 | 仕様 | 関連 AC |
|---|---|---|
| 初期状態 | `_drawerOpen = true`（Md 以上では永続表示） | AC-005 |
| トグル | AppBar のメニューアイコンで Drawer の開閉 | AC-006 |
| レスポンシブ | `DrawerVariant.Responsive` + `Breakpoint.Md`。Md 未満では MudBlazor 標準のオーバーレイ動作 | AC-005 / AC-006 / REQ-016 |
| アクティブ表示 | `MudNavLink Match` に `NavigationMatchExtensions.ToNavLinkMatch(item.Match)` を渡す。`Home (/)` は `All` 相当、それ以外は `Prefix`（GUD-004） | AC-010 |
| アイコン | `IconResolver.Resolve(item.Icon)` で SVG 解決。未知キーは `Help` アイコンへフォールバック（AC-008） | AC-008 |
| 並び順 | `OrderBy(item => item.Group ?? string.Empty).ThenBy(item => item.Order).ThenBy(item => item.Title, StringComparer.Ordinal)` を `OnInitialized` で 1 度だけ評価しキャッシュ（NFR-001） | AC-013 / AC-009 |
| エラー UI | 既存 `blazor-error-ui` div は `MainLayout` 内・`MudLayout` 外に維持（リセット時に表示が壊れないこと） | NFR-003 / AC-012 |

---

## 4. ナビゲーションの Self-Registration パターン

### 4.1 抽象（`SharedKernel.Application/Navigation`）

```csharp
namespace EventRegistration.SharedKernel.Application.Navigation;

public enum NavigationMatch
{
    Prefix = 0,
    All = 1,
}

public interface INavigationItem
{
    string Title { get; }
    string Href { get; }
    string Icon { get; }              // Shell 側 IconResolver で MudBlazor SVG に解決
    string? Group { get; }
    int Order { get; }
    NavigationMatch Match { get; }
}

public sealed record NavigationItem(
    string Title,
    string Href,
    string Icon,
    string? Group = null,
    int Order = 100,
    NavigationMatch Match = NavigationMatch.Prefix
) : INavigationItem;
```

- **配置理由**: `INavigationItem` を `SharedKernel.Application` に置くことで、各モジュールの Application 層は `Web` を参照することなく自モジュールのナビゲーション項目を提供できる（B-1 対応 / CON-006 厳守）。
- **UI 中立**: `Icon` は `string`、`Match` は自前 `NavigationMatch` enum とし、抽象から MudBlazor / `Microsoft.AspNetCore.Components.Routing` への依存を一切持ち込まない（CON-002 / B-2 対応 / PAT-004）。

### 4.2 モジュール側の登録 API

```csharp
// EventRegistration.Events.Application
public static class EventsModuleNavigationExtensions
{
    public static IServiceCollection AddEventsModuleNavigation(this IServiceCollection services);
}

// EventRegistration.Registrations.Application
public static class RegistrationsModuleNavigationExtensions
{
    public static IServiceCollection AddRegistrationsModuleNavigation(this IServiceCollection services);
}
```

- 各モジュールは `services.AddSingleton<INavigationItem>(new NavigationItem(...))` で **1 件以上の項目を Singleton 登録**する（GUD-002 / AC-015）。
- 本 PR の Walking Skeleton 段階では、いずれのモジュールも `Href = "/"`（プレースホルダー）でルートへ向ける（B-3 / 仕様スコープ外で `/events`・`/registrations` ページは別 Issue 化）。
- ライフタイム選定理由: ナビゲーション項目は **不変な静的メタデータ**であり、Singleton が最適。Scoped/Transient は無駄なアロケーションを発生させ NFR-002 の「O(1) 相当の登録」原則に反する。

### 4.3 Shell 側の解決

```razor
@inject IEnumerable<INavigationItem> NavigationItems
```

- 1 度だけ列挙して `OrderBy(...).ThenBy(...).ThenBy(...)` でソートし、フィールドにキャッシュする。
- グルーピングは `GroupBy(item => item.Group)` で行い、`null` グループは直置き、それ以外は `MudNavGroup` でまとめる。

### 4.4 拡張ポイント

| シナリオ | 対応 |
|---|---|
| 新モジュールのナビゲーション追加 | 当該モジュール `Application` に `AddXxxModuleNavigation()` を追加し、`Program.cs` から呼ぶだけ。Shell コードは無変更（PAT-001） |
| 1 モジュールから複数項目 | `services.AddSingleton<INavigationItem>(...)` を複数回呼ぶ |
| アイコンキーの追加 | `IconResolver` のマッピングに 1 行追加（Shell 側のみの変更 / AC-008） |

---

## 5. テーマ設定（`AppTheme` / `MudThemeProvider`）

<!-- TODO: Phase 3 で AppTheme.cs の最終確定値を反映 -->

### 5.1 配置

- ファイル: `EventRegistration.Web/Shell/Theme/AppTheme.cs`（`static class`、`internal` 想定）。
- 公開メンバー: `public static readonly MudTheme Light`。

### 5.2 配置理由

- `MudTheme` / `PaletteLight` / `Typography` は **MudBlazor 型に依存**するため、`Web`（Composition Root）にのみ存在させる（CON-002 厳守）。SharedKernel やモジュールには絶対に置かない。

### 5.3 主要パレット（予定値）

| プロパティ | 値 | 根拠 |
|---|---|---|
| `PaletteLight.Primary` | `#512BD4` | AC-007（.NET ブランドカラー） |
| `PaletteLight.AppbarBackground` | `#512BD4` | AC-007 |
| `PaletteLight.AppbarText` | `#FFFFFF` | コントラスト確保 |
| `PaletteDark` | 仮定義のみ | CON-005 拡張余地確保。本 PR では使用しない（`IsDarkMode=false` 固定 / AC-011） |

### 5.4 適用方法

- `MainLayout.razor` の最上位に `<MudThemeProvider Theme="AppTheme.Light" IsDarkMode="false" />` を配置（REQ-005 / REQ-014 / REQ-015）。

---

## 6. アイコン / Match 変換ヘルパー

<!-- TODO: Phase 3 で IconResolver の最終マッピング表を反映 -->

### 6.1 `IconResolver`（`Shell/Navigation/IconResolver.cs`、`internal static`）

- API: `public static string Resolve(string iconName)`
- マッピング（予定）: 文字列キー（例: `"Material.Filled.Event"`、`"Material.Filled.HowToReg"`、`"Material.Filled.Home"`、`"Material.Filled.People"`）→ `MudBlazor.Icons.Material.Filled.*` の SVG 文字列。
- 未知キー: `MudBlazor.Icons.Material.Filled.Help` の SVG にフォールバックし、例外を投げない（AC-008）。

### 6.2 `NavigationMatchExtensions`（同ディレクトリ、`internal static`）

- API: `public static NavLinkMatch ToNavLinkMatch(this NavigationMatch match)`
- マッピング: `Prefix → NavLinkMatch.Prefix` / `All → NavLinkMatch.All`（GUD-004）。

---

## 7. テスト方針

<!-- TODO: Phase 3 でテストプロジェクト追加後、ファイル一覧と実テスト数を反映 -->

### 7.1 ユニットテスト（MSTest 4.x + FluentAssertions / `csharp-mstest` スキル準拠）

| 対象 | テストプロジェクト | 主要テストケース | 関連 AC |
|---|---|---|---|
| `NavigationItem` record | `EventRegistration.SharedKernel.Application.Tests`（新規） | デフォルト値（`Order=100` / `Match=Prefix` / `Group=null`）、record の値ベース等価性 | AC-014 |
| `NavigationMatch` enum | 同上 | `Prefix=0`, `All=1` の数値固定（B-2 後方互換） | AC-014 |
| `AddEventsModuleNavigation` / `AddRegistrationsModuleNavigation` | `EventRegistration.Web.Tests`（新規）または各モジュール `Application.Tests` | 呼び出し後 `ServiceCollection` から `INavigationItem` が 1 件以上 Singleton として解決でき、`Href == "/"` であること | AC-015 |
| `IconResolver.Resolve` | `EventRegistration.Web.Tests` | 既知キーは対応 SVG / 未知キーは `Help` SVG | AC-008 |
| `NavigationMatchExtensions.ToNavLinkMatch` | 同上 | `Prefix → NavLinkMatch.Prefix` / `All → NavLinkMatch.All` | GUD-004 |
| Composition Root 統合 | 同上（最小ホスト + ServiceCollection 検証） | `IServiceProvider.GetServices<INavigationItem>()` が 2 件以上を含むこと | AC-002 / AC-003 |
| ナビゲーション並び替えロジック | 同上（純関数として切り出してテスト） | `Group → Order → Title` の昇順 | AC-013 / AC-009 |
| アーキテクチャテスト | 同上 | `Modules.*.Application/Infrastructure` および `SharedKernel.*` が `MudBlazor` および `EventRegistration.Web` を参照しないこと（NetArchTest または手書き `AssemblyName` 検査） | AC-014 / CON-002 / CON-006 |

### 7.2 UI レンダリングテスト（任意 / 状況により）

- 本 PR では bUnit による `MainLayout` 最小描画テスト（`MudNavLink` 数 ≥ 2、`MudAppBar` のアプリ名表示、`MudNavGroup` がグループ項目を内包する）を追加候補とする。
- MudBlazor の bUnit 対応状況に依存し導入コストが高い場合は、**本 PR では手動検証 + 単体ロジックテストに留め、E2E（Playwright）は別 Issue 化**する（仕様 §6 に記載のとおり、Playwright は本リポジトリ未導入）。

### 7.3 手動検証（AC-003 / AC-007 / AC-010 / AC-011）

- `dotnet run --project src/EventRegistration.Web` で起動し、AppBar・Drawer・テーマ色・アクティブハイライト・ライトモード固定をスクリーンショットで PR 説明に添付。
- Tester エージェントが Phase 3 終盤でシナリオ実行する。

### 7.4 ビルド・テスト実行コマンド

`EventRegistration.sln` はリポジトリルート直下にあるため、**リポジトリルートから** 実行する。

```bash
dotnet build EventRegistration.sln
dotnet test EventRegistration.sln
```

### 7.5 カバレッジ目標

- Shell/Navigation 関連クラス（`IconResolver`、`NavigationMatchExtensions`、ナビゲーション並び替え、各 `AddXxxModuleNavigation`）の **行カバレッジ 80% 以上**（仕様 §6 Coverage Requirements）。

---

## 8. TODO（Phase 3 / Step 3.4.5 で更新する箇所）

- [ ] MudBlazor の確定バージョンを §2.1 に記載
- [ ] `App.razor` の最終 head/body 差分を §2.3 に追記
- [ ] `MainLayout.razor` の最終 razor 全文を §3 に埋め込み
- [ ] `AppTheme.Light` の最終定義（Typography 含む）を §5.3 に反映
- [ ] `IconResolver` の最終マッピング表を §6.1 に反映
- [ ] テストプロジェクト構成の実態と実テスト数を §1 ディレクトリ構成（`src/tests/`）および §7.1 に反映
- [ ] 関連コメントの正式 URL（`#issuecomment-XXXX`）を「概要」末尾に反映
- [ ] PR 番号を冒頭メタ情報に反映
