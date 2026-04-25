# 非同期コンポーネントのテスト

`OnInitializedAsync` 等で非同期処理を行うコンポーネントのテストパターン。

---

## WaitForAssertion — アサーションの成功を待機

コンポーネントが再レンダリングされるたびにアサーションを再評価し、成功するまで待機する。

```csharp
var cut = Render<AsyncDataComponent>();

cut.WaitForAssertion(() =>
    cut.Find("p").MarkupMatches("<p>Data loaded</p>")
);
```

### タイムアウト制御

デフォルトは 1 秒。デバッガ接続時は自動で無効化される。

```csharp
cut.WaitForAssertion(
    () => cut.Find("p").MarkupMatches("<p>Done</p>"),
    TimeSpan.FromSeconds(5)
);
```

---

## WaitForState — 条件の成立を待機

述語が `true` を返すまで待機する。アサーションとは別に「状態の到達」を待つために使用。

```csharp
var cut = Render<AsyncDataComponent>();

cut.WaitForState(() => cut.Find("p").TextContent == "Data loaded");
cut.MarkupMatches("<p>Data loaded</p>");
```

> **注意**: `WaitForState` と同じ内容をアサーションで検証する場合は `WaitForAssertion` を使うこと。

---

## WaitForElement / WaitForElements — DOM 要素の出現を待機

特定の CSS セレクタに一致する要素がレンダリングされるまで待機する。

```csharp
// 単一要素の待機
var element = cut.WaitForElement("button.submit");

// 複数要素の待機 (最低 N 個)
var items = cut.WaitForElements("li", 3);
Assert.AreEqual(3, items.Count);
```

---

## WaitForComponent / WaitForComponents — 子コンポーネントの出現を待機

指定した型のコンポーネントがレンダリングされるまで待機する。

```csharp
// 単一コンポーネント
var listItem = cut.WaitForComponent<ListItem>();
Assert.AreEqual("Item 1", listItem.Find(".list-item").TextContent);

// 複数コンポーネント (最低 N 個)
var items = cut.WaitForComponents<ListItem>(3);
Assert.AreEqual(3, items.Count);
```

---

## TaskCompletionSource パターン

非同期依存を `TaskCompletionSource<T>` で制御し、テスト内でタイミングを明示的にコントロールする。

```csharp
var tcs = new TaskCompletionSource<string>();
var cut = Render<AsyncData>(parameters => parameters
    .Add(p => p.DataService, tcs.Task)
);

// この時点ではコンポーネントは Loading 表示
cut.Find("p").MarkupMatches("<p>Loading...</p>");

// データ返却をシミュレート
tcs.SetResult("Hello World");

// 再レンダリングを待機して検証
cut.WaitForAssertion(() =>
    cut.Find("p").MarkupMatches("<p>Hello World</p>")
);
```

---

## エラーケースのテスト

```csharp
var tcs = new TaskCompletionSource<string>();
var cut = Render<AsyncData>(parameters => parameters
    .Add(p => p.DataService, tcs.Task)
);

// 例外をシミュレート
tcs.SetException(new HttpRequestException("Network error"));

cut.WaitForAssertion(() =>
    cut.Find(".error").MarkupMatches("<div class=\"error\">Network error</div>")
);
```
