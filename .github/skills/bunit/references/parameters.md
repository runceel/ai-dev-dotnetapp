# パラメータの渡し方

bUnit 2.x の `Render<T>()` メソッドでは、`ComponentParameterCollectionBuilder<T>` を使って型安全にパラメータを渡す。

## 基本パラメータ

```csharp
var cut = Render<Alert>(parameters => parameters
    .Add(p => p.Heading, "Warning")
    .Add(p => p.Type, AlertType.Danger)
);
```

`Add` メソッドはラムダ式でパラメータを選択するため、**型安全**かつ**リファクタリング対応**。

## EventCallback パラメータ

```csharp
var clicked = false;
var cut = Render<MyButton>(parameters => parameters
    .Add(p => p.OnClick, (MouseEventArgs _) => clicked = true)
);

cut.Find("button").Click();
Assert.IsTrue(clicked);
```

## Bind パラメータ

`@bind-Value` 相当の双方向バインディング:

```csharp
var currentValue = "initial";
var cut = Render<MyInput>(parameters => parameters
    .Bind(p => p.Value, currentValue, newValue => currentValue = newValue)
);

cut.Find("input").Change("updated");
Assert.AreEqual("updated", currentValue);
```

## ChildContent (RenderFragment)

```csharp
// HTML 文字列を渡す
var cut = Render<Card>(parameters => parameters
    .AddChildContent("<p>Hello World</p>")
);

// 子コンポーネントを渡す
var cut = Render<Card>(parameters => parameters
    .AddChildContent<Badge>(childParams => childParams
        .Add(p => p.Label, "New")
    )
);

// HTML + コンポーネントの混在
var cut = Render<Card>(parameters => parameters
    .AddChildContent("<h1>Title</h1>")
    .AddChildContent<Badge>(childParams => childParams
        .Add(p => p.Label, "New")
    )
);
```

## 名前付き RenderFragment

```csharp
var cut = Render<Tabs>(parameters => parameters
    .Add<TabPanel>(p => p.Header, tabParams => tabParams
        .Add(p => p.Title, "Tab 1")
    )
);
```

## RenderFragment&lt;TValue&gt; (テンプレートパラメータ)

```csharp
var cut = Render<DataList<string>>(parameters => parameters
    .Add(p => p.Items, new[] { "Item1", "Item2" })
    .Add(p => p.ItemTemplate, item =>
        $"<li>{item}</li>")
);
```

## CascadingValue

```csharp
var cut = Render<MyComponent>(parameters => parameters
    .Add(p => p.Theme, "dark")  // [CascadingParameter] の場合
);
```

## Unmatched パラメータ

```csharp
var cut = Render<MyInput>(parameters => parameters
    .AddUnmatched("class", "form-control")
    .AddUnmatched("data-testid", "name-input")
);
```

## パラメータの再設定 (再レンダリング)

```csharp
var cut = Render<Counter>(parameters => parameters
    .Add(p => p.InitialCount, 0)
);

// 新しいパラメータで再レンダリング
cut.Render(parameters => parameters
    .Add(p => p.InitialCount, 10)
);
```
