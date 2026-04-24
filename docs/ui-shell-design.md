# UI Shell 設計（MudBlazor 基盤 + ナビゲーション Self-Registration）

> 対象: イベント参加登録システム UI Shell 基盤（SPEC: [event-registration-system-spec.md](./event-registration-system-spec.md) / Issue: [#3](https://github.com/runceel/ai-dev-dotnetapp/issues/3) / PR: [#4](https://github.com/runceel/ai-dev-dotnetapp/pull/4)）
> ステータス: **実装反映済み（Phase 3 / Step 3.4.5）**
> 関連: [architecture.md](./architecture.md)（Walking Skeleton 全体構造）

---

## 概要

本ドキュメントは、`EventRegistration.Web` に **MudBlazor を導入し、AppBar + 折りたたみ Drawer + メインコンテンツ領域からなる UI Shell** を構築した設計を定義する。あわせて、各業務モジュール（`Events` / `Registrations`）が **自身のナビゲーション項目を DI 経由で自己登録**する `INavigationItem` 抽象（Self-Registration パターン / PAT-001）を導入し、Shell 側はモジュールの追加・削除に対してコード変更不要で項目を表示できるようにした。

### 主要なポイント

| 項目 | 内容 |
|------|------|
| UI フレームワーク | **MudBlazor 9.4.0**（`net10.0` 互換） |
| Shell 構成 | `MudThemeProvider` + Provider 群（`Popover` / `Dialog` / `Snackbar`）→ `MudLayout`（`MudAppBar` / `MudDrawer` / `MudMainContent`） |
| ナビゲーション抽象 | `INavigationItem`（`SharedKernel.Application/Navigation`） |
| ナビゲーション登録方式 | Self-Registration（モジュール側 `AddXxxModuleNavigation()` で `INavigationItem` を Singleton 登録） |
| Shell 側解決方法 | `@inject IEnumerable<INavigationItem>` をレイアウトコンポーネントに注入 |
| テーマ | `EventRegistration.Web/Shell/Theme/AppTheme.cs`（`MudTheme Light`、Primary `#512BD4`） |
| アイコン解決 | `EventRegistration.Web/Shell/Navigation/IconResolver.cs`（短い文字列キー → `MudBlazor.Icons.Material.Filled.*` SVG） |
| Match 変換 | `NavigationMatchExtensions.ToNavLinkMatch()`（`NavigationMatch` → `NavLinkMatch`） |
| 依存方向 | `Web → Modules.Application → SharedKernel.Application` のみ（CON-006 厳守） |
| Composition Root | `EventRegistration.Web/Program.cs`（`AddMudServices()` と各 `AddXxxModuleNavigation()` 呼び出し） |

### 関連ドキュメント / コメント

- 仕様: [event-registration-system-spec.md](./event-registration-system-spec.md)
- 全体アーキテクチャ: [architecture.md](./architecture.md)
- Issue: [#3](https://github.com/runceel/ai-dev-dotnetapp/issues/3)
- PR: [#4](https://github.com/runceel/ai-dev-dotnetapp/pull/4)

---

## 1. ディレクトリ構成（実装反映後）

```
ai-dev-dotnetapp/
├── docs/
│   ├── architecture.md
│   ├── event-registration-system-spec.md
│   └── ui-shell-design.md                          # 本ドキュメント
└── src/
    ├── EventRegistration.Web/
    │   ├── Program.cs                              # AddMudServices() / AddEventsModuleNavigation() / AddRegistrationsModuleNavigation()
    │   ├── EventRegistration.Web.csproj            # MudBlazor 9.4.0 PackageReference 追加
    │   ├── Components/
    │   │   ├── App.razor                           # MudBlazor CSS/JS / Roboto フォント参照を追加
    │   │   ├── _Imports.razor                      # MudBlazor / Shell.* / SharedKernel.Application.Navigation の using
    │   │   ├── Layout/
    │   │   │   ├── MainLayout.razor                # MudLayout 構造へ刷新（既存 blazor-error-ui は MudLayout 外に維持）
    │   │   │   └── MainLayout.razor.css            # MudBlazor がレイアウトを担うため Blazor 標準エラー UI 用スタイルのみ
    │   │   └── Pages/
    │   │       ├── Home.razor                      # MudText ベースに刷新（日本語化）
    │   │       ├── Error.razor                     # MudAlert / MudText ベースに刷新（日本語化）
    │   │       └── NotFound.razor                  # MudAlert / MudText ベースに刷新（日本語化）
    │   ├── Shell/                                  # （新規）UI Shell 専用領域
    │   │   ├── Theme/
    │   │   │   └── AppTheme.cs                     # MudTheme Light（Primary=#512BD4 ほか / Roboto Typography）
    │   │   └── Navigation/
    │   │       ├── IconResolver.cs                 # 文字列キー → MudBlazor SVG 解決（public static）
    │   │       └── NavigationMatchExtensions.cs    # NavigationMatch → NavLinkMatch 変換（public static）
    │   └── wwwroot/
    │       └── app.css                             # MudBlazor 移行に伴い Bootstrap 由来スタイルを除去
    └── Modules/
        ├── SharedKernel/
        │   └── EventRegistration.SharedKernel.Application/
        │       └── Navigation/                     # （新規）UI 中立ナビゲーション抽象
        │           ├── INavigationItem.cs
        │           ├── NavigationItem.cs           # sealed record
        │           └── NavigationMatch.cs          # enum { Prefix, All }
        ├── Events/
        │   └── EventRegistration.Events.Application/
        │       ├── EventRegistration.Events.Application.csproj   # SharedKernel.Application 参照 + DI.Abstractions 10.0.0
        │       └── Navigation/
        │           └── EventsNavigationExtensions.cs              # AddEventsModuleNavigation()
        └── Registrations/
            └── EventRegistration.Registrations.Application/
                ├── EventRegistration.Registrations.Application.csproj  # SharedKernel.Application 参照 + DI.Abstractions 10.0.0
                └── Navigation/
                    └── RegistrationsNavigationExtensions.cs            # AddRegistrationsModuleNavigation()
```

> 注: 本 PR ではテストプロジェクトは追加していない。Walking Skeleton 段階の最小スコープに合わせ、`AddEventsModuleNavigation()` / `AddRegistrationsModuleNavigation()` / `IconResolver` / `NavigationMatchExtensions` などのユニットテスト・アーキテクチャテストは次フェーズの Issue として切り出す（§7 参照）。

> 注: 各モジュール `Application` プロジェクトには `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0 への PackageReference を追加し、`IServiceCollection` 拡張メソッドの実装に必要な最小依存とした。`MudBlazor` および `EventRegistration.Web` への参照は **モジュール側からは一切持たない**（CON-002 / CON-006 / AC-014）。

---

## 2. MudBlazor 導入設定

### 2.1 NuGet パッケージ参照

`EventRegistration.Web.csproj` に以下を追加。

```xml
<ItemGroup>
  <PackageReference Include="MudBlazor" Version="9.4.0" />
</ItemGroup>
```

- 既存の `BlazorDisableThrowNavigationException` 等の `PropertyGroup` 設定は **改変せず維持**（CON-004）。
- モジュール側プロジェクトには MudBlazor を **追加していない**。

### 2.2 `Program.cs`（Composition Root）

```csharp
using EventRegistration.Events.Application.Navigation;
using EventRegistration.Registrations.Application.Navigation;
using EventRegistration.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// MudBlazor の各種サービスを登録
builder.Services.AddMudServices();

// 各モジュールが提供するナビゲーション項目を登録
builder.Services.AddEventsModuleNavigation();
builder.Services.AddRegistrationsModuleNavigation();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

- `AddMudServices()` は `EventRegistration.Web/Program.cs` でのみ呼ぶ（CON-001 Composition Root 単一原則）。
- 各モジュールの `AddXxxModuleNavigation()` は **同期的に Singleton で `INavigationItem` を登録**する（順序非依存）。

### 2.3 `Components/App.razor`

`<head>` に MudBlazor CSS と Google Fonts の Roboto を追加し、`<body>` 末尾の `blazor.web.js` 直前に MudBlazor JS を追加した（差分のみ抜粋）。

```razor
<head>
    ...
    <ResourcePreloader />
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="@Assets["app.css"]" />
    ...
</head>
<body>
    <Routes />
    <ReconnectModal />
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
    <script src="@Assets["_framework/blazor.web.js"]"></script>
</body>
```

### 2.4 `Components/_Imports.razor`

Razor 共通 `using` に MudBlazor および Shell / SharedKernel ナビゲーション名前空間を追加。

```razor
@using MudBlazor
@using EventRegistration.SharedKernel.Application.Navigation
@using EventRegistration.Web.Shell.Navigation
@using EventRegistration.Web.Shell.Theme
```

---

## 3. UI Shell 構成（`MainLayout.razor`）

### 3.1 実装

```razor
@inherits LayoutComponentBase
@inject IEnumerable<INavigationItem> NavigationItems

<MudThemeProvider Theme="AppTheme.Light" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="ToggleDrawer" />
        <MudText Typo="Typo.h6" Class="ml-3">イベント参加登録システム</MudText>
        <MudSpacer />
    </MudAppBar>

    <MudDrawer @bind-Open="_drawerOpen" ClipMode="DrawerClipMode.Always" Elevation="2" Variant="DrawerVariant.Responsive">
        <MudDrawerHeader>
            <MudText Typo="Typo.h6">メニュー</MudText>
        </MudDrawerHeader>
        <MudNavMenu>
            <MudNavLink Href="/" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">ホーム</MudNavLink>
            @foreach (var group in NavigationItems
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Order)
                .ThenBy(x => x.Title)
                .GroupBy(x => x.Group))
            {
                <MudNavGroup Title="@group.Key" Expanded="true">
                    @foreach (var item in group)
                    {
                        <MudNavLink Href="@item.Href"
                                    Match="@NavigationMatchExtensions.ToNavLinkMatch(item.Match)"
                                    Icon="@IconResolver.Resolve(item.Icon)">
                            @item.Title
                        </MudNavLink>
                    }
                </MudNavGroup>
            }
        </MudNavMenu>
    </MudDrawer>

    <MudMainContent Class="pt-16 px-4">
        @Body
    </MudMainContent>
</MudLayout>

<div id="blazor-error-ui" data-nosnippet>
    An unhandled error has occurred.
    <a href="." class="reload">Reload</a>
    <span class="dismiss">🗙</span>
</div>

@code {
    private bool _drawerOpen = true;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}
```

### 3.2 動作仕様

| 項目 | 仕様 | 関連 AC |
|---|---|---|
| 初期状態 | `_drawerOpen = true`（Md 以上では永続表示） | AC-005 |
| トグル | AppBar のメニューアイコン (`Icons.Material.Filled.Menu`) で Drawer を開閉 | AC-006 |
| レスポンシブ | `DrawerVariant.Responsive` + `ClipMode.Always`。Md 未満では MudBlazor 標準のオーバーレイ動作 | AC-005 / AC-006 / REQ-016 |
| アクティブ表示 | `MudNavLink Match` に `NavigationMatchExtensions.ToNavLinkMatch(item.Match)` を渡す。`Home (/)` は `NavLinkMatch.All`、それ以外はモジュール側が `NavigationMatch.Prefix` を指定（GUD-004） | AC-010 |
| アイコン | `IconResolver.Resolve(item.Icon)` で SVG 解決。未知キーは `Icons.Material.Filled.Help` へフォールバック（AC-008） | AC-008 |
| 並び順 | `OrderBy(x => x.Group).ThenBy(x => x.Order).ThenBy(x => x.Title)` でソートし、`GroupBy(x => x.Group)` で `MudNavGroup` にまとめる | AC-013 / AC-009 |
| エラー UI | 既存 `blazor-error-ui` div は `MainLayout` 内・`MudLayout` 外に維持 | NFR-003 / AC-012 |

> 補足: 並び替えは現在 Razor テンプレート内の式で行っているため、`MainLayout` の再レンダリング毎に評価される。Walking Skeleton 段階では項目数が少なく無視できるが、項目数が増えた場合は `OnInitialized` でフィールドにキャッシュするリファクタリングを検討する（NFR-001 / 仕様 §6）。

---

## 4. ナビゲーションの Self-Registration パターン

### 4.1 抽象（`SharedKernel.Application/Navigation`）

```csharp
namespace EventRegistration.SharedKernel.Application.Navigation;

public enum NavigationMatch
{
    /// <summary>現在のパスが Href で始まる場合にアクティブ。</summary>
    Prefix,

    /// <summary>現在のパスが Href と完全一致する場合にアクティブ。</summary>
    All,
}

public interface INavigationItem
{
    string Title { get; }
    string Href { get; }
    string Icon { get; }              // Shell 側 IconResolver で MudBlazor SVG に解決
    string Group { get; }             // MudNavGroup 名（必須）
    int Order { get; }                // 同一グループ内の表示順
    NavigationMatch Match { get; }
}

public sealed record NavigationItem(
    string Title,
    string Href,
    string Icon,
    string Group,
    int Order,
    NavigationMatch Match) : INavigationItem;
```

- **配置理由**: `INavigationItem` を `SharedKernel.Application` に置くことで、各モジュールの Application 層は `Web` を参照することなく自モジュールのナビゲーション項目を提供できる（CON-006 厳守）。
- **UI 中立**: `Icon` は `string`、`Match` は自前 `NavigationMatch` enum とし、抽象から MudBlazor / `Microsoft.AspNetCore.Components.Routing` への依存を一切持ち込まない（CON-002 / PAT-004）。
- **Group は必須**: 実装上 `Group` を非 null としており、Shell は常に `MudNavGroup` で項目をまとめる。`Home` などグルーピング不要な項目は Shell 側でハードコード済み（§3.1 参照）。

### 4.2 モジュール側の登録 API

```csharp
// EventRegistration.Events.Application
public static class EventsNavigationExtensions
{
    public static IServiceCollection AddEventsModuleNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationItem>(new NavigationItem(
            Title: "イベント管理",
            Href: "/", // プレースホルダー (将来 /events 等に差し替え)
            Icon: "Event",
            Group: "イベント",
            Order: 100,
            Match: NavigationMatch.Prefix));
        return services;
    }
}

// EventRegistration.Registrations.Application
public static class RegistrationsNavigationExtensions
{
    public static IServiceCollection AddRegistrationsModuleNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationItem>(new NavigationItem(
            Title: "参加登録",
            Href: "/", // プレースホルダー (将来 /registrations 等に差し替え)
            Icon: "HowToReg",
            Group: "参加者",
            Order: 200,
            Match: NavigationMatch.Prefix));
        return services;
    }
}
```

- 各モジュールは `services.AddSingleton<INavigationItem>(new NavigationItem(...))` で **1 件以上の項目を Singleton 登録**する（GUD-002 / AC-015）。
- 本 PR の Walking Skeleton 段階では、いずれのモジュールも `Href = "/"`（プレースホルダー）でルートへ向ける。`/events` / `/registrations` ページの実装は別 Issue で扱う。
- ライフタイム選定理由: ナビゲーション項目は **不変な静的メタデータ**であり、Singleton が最適（NFR-002）。

### 4.3 Shell 側の解決

```razor
@inject IEnumerable<INavigationItem> NavigationItems
```

- `OrderBy(x => x.Group).ThenBy(x => x.Order).ThenBy(x => x.Title).GroupBy(x => x.Group)` で並び替え＋グルーピング。
- グルーピング結果ごとに `MudNavGroup`（`Expanded="true"`）を生成し、配下に `MudNavLink` を並べる。

### 4.4 拡張ポイント

| シナリオ | 対応 |
|---|---|
| 新モジュールのナビゲーション追加 | 当該モジュール `Application` に `AddXxxModuleNavigation()` を追加し、`Program.cs` から呼ぶだけ。Shell コードは無変更（PAT-001） |
| 1 モジュールから複数項目 | `services.AddSingleton<INavigationItem>(...)` を複数回呼ぶ |
| アイコンキーの追加 | `IconResolver` の `IconMap` に 1 行追加（Shell 側のみの変更 / AC-008） |

---

## 5. テーマ設定（`AppTheme` / `MudThemeProvider`）

### 5.1 配置

- ファイル: `EventRegistration.Web/Shell/Theme/AppTheme.cs`（`public static class`）。
- 公開メンバー: `public static readonly MudTheme Light`。

### 5.2 配置理由

- `MudTheme` / `PaletteLight` / `Typography` は **MudBlazor 型に依存**するため、`Web`（Composition Root）にのみ存在させる（CON-002 厳守）。SharedKernel やモジュールには配置しない。

### 5.3 実装

```csharp
public static class AppTheme
{
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
```

| プロパティ | 値 | 根拠 |
|---|---|---|
| `PaletteLight.Primary` / `AppbarBackground` | `#512BD4` | AC-007（.NET ブランドカラー） |
| `PaletteLight.Secondary` | `#68217A` | Visual Studio Purple（補色） |
| `PaletteLight.Tertiary` | `#9B4DCA` | アクセント |
| `PaletteLight.DrawerBackground` / `DrawerText` | `#F5F5F5` / `#424242` | AppBar とのコントラスト確保 |
| `Typography.Default.FontFamily` | `Roboto, Helvetica, Arial, sans-serif` | Material Design 既定 |

### 5.4 適用方法

- `MainLayout.razor` の最上位に `<MudThemeProvider Theme="AppTheme.Light" />` を配置（REQ-005 / REQ-014 / REQ-015）。
- 本 PR では `IsDarkMode` を明示指定しておらず、MudBlazor の既定（ライト）に従う（AC-011）。ダークモードのトグル UI は本スコープ外。

---

## 6. アイコン / Match 変換ヘルパー

### 6.1 `IconResolver`（`Shell/Navigation/IconResolver.cs`、`public static`）

```csharp
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

    public static string Resolve(string iconKey)
    {
        return IconMap.TryGetValue(iconKey, out var icon) ? icon : Icons.Material.Filled.Help;
    }
}
```

- API: `public static string Resolve(string iconKey)`
- マッピング: 短い文字列キー（`Home` / `Event` / `HowToReg` ほか）→ `MudBlazor.Icons.Material.Filled.*` の SVG 文字列。
- キー比較は `StringComparer.OrdinalIgnoreCase`。
- 未知キー: `Icons.Material.Filled.Help` SVG にフォールバックし、例外を投げない（AC-008）。
- アイコンキーを追加する場合は `IconMap` に 1 行追加するだけで Shell・モジュール双方の I/F は影響を受けない。

### 6.2 `NavigationMatchExtensions`（同ディレクトリ、`public static`）

```csharp
public static class NavigationMatchExtensions
{
    public static NavLinkMatch ToNavLinkMatch(NavigationMatch match) => match switch
    {
        NavigationMatch.All => NavLinkMatch.All,
        NavigationMatch.Prefix => NavLinkMatch.Prefix,
        _ => NavLinkMatch.Prefix,
    };
}
```

- API: `public static NavLinkMatch ToNavLinkMatch(NavigationMatch match)`（拡張メソッドではなく静的メソッド呼び出しで利用）。
- マッピング: `Prefix → NavLinkMatch.Prefix` / `All → NavLinkMatch.All`、未知値はデフォルトで `Prefix`（GUD-004）。

---

## 7. テスト方針

本 PR ではテストプロジェクトを追加していない（Walking Skeleton 最小スコープ）。以下のテストは次フェーズの Issue として切り出して追加する想定。

### 7.1 追加予定のユニットテスト（MSTest 4.x + FluentAssertions / `csharp-mstest` スキル準拠）

| 対象 | 想定テストプロジェクト | 主要テストケース | 関連 AC |
|---|---|---|---|
| `NavigationItem` record | `EventRegistration.SharedKernel.Application.Tests` | record の値ベース等価性・各プロパティ反映 | AC-014 |
| `NavigationMatch` enum | 同上 | `Prefix` / `All` の値が安定していること | AC-014 |
| `AddEventsModuleNavigation` / `AddRegistrationsModuleNavigation` | `EventRegistration.Events.Application.Tests` / `EventRegistration.Registrations.Application.Tests` | 呼び出し後 `ServiceCollection` から `INavigationItem` が 1 件以上 Singleton として解決でき、各 `Title` / `Group` / `Icon` が期待値であること | AC-015 |
| `IconResolver.Resolve` | `EventRegistration.Web.Tests` | 既知キー（大小文字・全エントリ）→ 対応 SVG / 未知キー → `Help` SVG | AC-008 |
| `NavigationMatchExtensions.ToNavLinkMatch` | 同上 | `Prefix → NavLinkMatch.Prefix` / `All → NavLinkMatch.All` | GUD-004 |
| Composition Root 統合 | 同上 | `IServiceProvider.GetServices<INavigationItem>()` が 2 件以上含むこと | AC-002 / AC-003 |
| アーキテクチャテスト | 同上 | `Modules.*.Application/Infrastructure` および `SharedKernel.*` が `MudBlazor` および `EventRegistration.Web` を参照しないこと | AC-014 / CON-002 / CON-006 |

### 7.2 UI レンダリングテスト（任意）

- bUnit による `MainLayout` 最小描画テスト（`MudNavLink` 数 ≥ 3、`MudAppBar` のアプリ名表示、`MudNavGroup` がグループ項目を内包する）は別 Issue 候補。
- MudBlazor の bUnit 対応コストが高い場合は、手動検証 + 単体ロジックテスト + Tester による E2E シナリオで代替する。

### 7.3 手動検証（AC-003 / AC-007 / AC-010 / AC-011）

- `dotnet run --project src/EventRegistration.AppHost` で Aspire 経由で起動し、AppBar・Drawer・テーマ色（`#512BD4`）・アクティブハイライト・ライトモード固定を確認する。
- Tester エージェントが Phase 3 終盤でシナリオ実行する。

### 7.4 ビルド・テスト実行コマンド

`EventRegistration.sln` はリポジトリルート直下にあるため、**リポジトリルートから** 実行する。

```bash
dotnet build EventRegistration.sln
dotnet test EventRegistration.sln
```

### 7.5 カバレッジ目標

- 次フェーズでテスト追加する際は、Shell/Navigation 関連クラス（`IconResolver`、`NavigationMatchExtensions`、各 `AddXxxModuleNavigation`）の **行カバレッジ 80% 以上**を目標とする（仕様 §6 Coverage Requirements）。

---

## 8. 既知の制約・今後の検討事項

- **Href プレースホルダー**: `Events` / `Registrations` モジュールの `Href` はいずれも `/`（ホーム）に向いており、実際の専用ページは未実装。後続 Issue で `/events` / `/registrations` ページを追加した時点で各 `AddXxxModuleNavigation()` の `Href` を差し替える。
- **テスト未追加**: 本 PR スコープでは UI Shell の Razor / 拡張クラスのユニットテストを追加していない（§7 参照）。
- **ナビゲーション並び替えの再評価**: 現状は Razor テンプレートで毎レンダー実行。項目数が増えた場合は `OnInitialized` でフィールドキャッシュ化する。
- **ダークモード対応**: `IsDarkMode` トグル UI とパレット切り替えは本スコープ外。
- **`Group` 必須化**: 仕様初版では `Group` を nullable として「グループ無しは直置き」としていたが、実装では非 null に統一し `Home` 項目のみ Shell 側でハードコードする方針に変更した。
