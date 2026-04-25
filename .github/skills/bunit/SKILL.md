---
name: bunit
description: 'bUnit 2.x を使用した Blazor コンポーネントのユニットテストのベストプラクティスとガイドライン'
---

# bUnit 2.x — Blazor コンポーネントテストガイド

bUnit 2.x (現行最新 v2.7.x) を使った Blazor コンポーネントのユニットテスト作成を支援する。
テストコードは **C# (.cs) ファイル**で記述し、テストフレームワークには **MSTest** を使用する。

> **重要**: bUnit 2.x は **.NET 8 以上** を必須とする。

詳細なリファレンスは `references/` フォルダを参照 — 必要に応じてオンデマンドで読み込む。

| リファレンス | 内容 |
|---|---|
| [parameters.md](references/parameters.md) | パラメータ渡しの全パターン (EventCallback, ChildContent, Bind, Cascading 等) |
| [test-doubles.md](references/test-doubles.md) | JSInterop, NavigationManager, 認証, HttpClient, ComponentStub |
| [async-testing.md](references/async-testing.md) | WaitForAssertion, WaitForState, WaitForElement, TaskCompletionSource |

---

## 1. プロジェクトセットアップ

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="bunit" Version="2.7.*" />
    <PackageReference Include="MSTest" Version="4.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp\MyApp.csproj" />
  </ItemGroup>
</Project>
```

> SDK は `Microsoft.NET.Sdk.Razor` を使用すること。

---

## 2. テストの基本構造

### BunitContext を継承 (推奨)

```csharp
[TestClass]
public sealed class CounterTests : BunitContext
{
    [TestMethod]
    public void Counter_InitialRender_DisplaysZero()
    {
        // Act
        var cut = Render<Counter>();

        // Assert
        cut.Find("p").MarkupMatches("<p>Current count: 0</p>");
    }
}
```

### BunitContext を都度生成

```csharp
[TestMethod]
public void Counter_InitialRender_DisplaysZero()
{
    using var ctx = new BunitContext();
    var cut = ctx.Render<Counter>();
    cut.MarkupMatches("<p>Current count: 0</p>");
}
```

---

## 3. コア API クイックリファレンス

### レンダリング

```csharp
var cut = Render<MyComponent>();                          // パラメータなし
var cut = Render<MyComponent>(p => p.Add(x => x.Name, "test")); // パラメータ付き
cut.Render(p => p.Add(x => x.Name, "updated"));          // 再レンダリング
```

### マークアップ検証

```csharp
cut.MarkupMatches("<p>Hello</p>");              // セマンティック HTML 比較
cut.Find("h1").MarkupMatches("<h1>Title</h1>"); // 要素単位
Assert.AreEqual("Hello", cut.Find("p").TextContent);
```

### 要素・コンポーネント検索

```csharp
var btn = cut.Find("button.primary");           // CSS セレクタ
var items = cut.FindAll("li");                  // 複数 (自動更新なし)
var child = cut.FindComponent<TaskItem>();       // 子コンポーネント
var children = cut.FindComponents<TaskItem>();   // 複数
```

### イベント

```csharp
cut.Find("button").Click();
cut.Find("input").Change("new value");
cut.Find("form").Submit();
cut.Find("input").TriggerEvent("oncustompaste", new CustomPasteEventArgs { ... });
```

### 非同期待機

```csharp
cut.WaitForAssertion(() => cut.Find("p").MarkupMatches("<p>Done</p>"));
cut.WaitForState(() => cut.Instance.IsLoaded);
cut.WaitForElement("button.submit");
cut.WaitForElements("li", 3);
cut.WaitForComponent<ListItem>();
```

### サービス登録

```csharp
Services.AddSingleton<IMyService>(new MockMyService());
JSInterop.Mode = JSRuntimeMode.Loose;  // JS 呼び出しを緩和
ComponentFactories.AddStub<HeavyChild>(); // 子コンポーネントをスタブ化
```

---

## 4. ベストプラクティス

### 構造

- テストクラスは `sealed` にする
- AAA パターン (Arrange-Act-Assert)
- テスト名: `ComponentName_Scenario_ExpectedBehavior`
- 変数名 `cut` を使用

### マークアップ検証

- 再利用コンポーネント → `MarkupMatches` で構造を検証
- アプリ固有コンポーネント → `Find` + `TextContent` / `Instance` でセマンティックに検証
- **不要な詳細を検証しない** — 脆いテストの原因になる

### 非同期

- `OnInitializedAsync` を持つコンポーネントには必ず `WaitForAssertion` / `WaitForState` を使用
- `TaskCompletionSource<T>` で非同期依存をシミュレート

### サービス・テストダブル

- テスト対象の依存は **必ず** `Services` に登録
- JSInterop は **デフォルトで Strict モード** — 必要な呼び出しは事前に `Setup` する
- 子コンポーネントの分離には `ComponentFactories.AddStub<T>()` を使用

### 避けるべきこと

- ❌ `FindAll` の結果を再利用せずに更新を期待する（再度 `FindAll` を呼ぶこと）
- ❌ JSInterop の `Setup` なしに JS 呼び出しを持つコンポーネントをレンダリング

