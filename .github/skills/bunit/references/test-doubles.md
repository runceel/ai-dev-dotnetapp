# テストダブル (組み込み)

bUnit 2.x に組み込まれたテストダブルの詳細リファレンス。

---

## JSInterop

bUnit は独自の `IJSRuntime` 実装を**デフォルトで提供**。追加登録は不要。

### モード設定

```csharp
// Strict モード (デフォルト) — 未設定の呼び出しで例外
JSInterop.Setup<string>("getTitle").SetResult("My Page");

// Loose モード — 未設定の呼び出しはデフォルト値を返す
JSInterop.Mode = JSRuntimeMode.Loose;
```

### Void メソッド

```csharp
var planned = JSInterop.SetupVoid("scrollToTop");
// ... コンポーネントの操作 ...
planned.SetVoidResult(); // 完了を通知
```

### JS モジュール

```csharp
var moduleInterop = JSInterop.SetupModule("./my-module.js");
moduleInterop.SetupVoid("initialize");
moduleInterop.Setup<int>("getValue").SetResult(42);

// モジュール単位でモードを変更可能
moduleInterop.Mode = JSRuntimeMode.Loose;
```

### IJSObjectReference

`IJSObjectReference` を返すメソッドのセットアップ:

```csharp
var objectRef = JSInterop.SetupModule(
    matcher => matcher.Identifier == "SomeModule.GetInstance");
objectRef.SetupVoid("doSomething");
```

### 呼び出し検証

```csharp
JSInterop.VerifyInvoke("scrollToTop");
JSInterop.VerifyInvoke("scrollToTop", calledTimes: 1);

// planned invocation 経由
var planned = JSInterop.Setup<string>("getTitle").SetResult("Title");
// ... テスト実行 ...
Assert.AreEqual(1, planned.Invocations.Count);
Assert.AreEqual("getTitle", planned.Invocations[0].Identifier);
```

### FocusAsync 検証

```csharp
var cut = Render<ClickToFocus>();
var inputElement = cut.Find("input");

cut.Find("button").Click();

JSInterop.VerifyFocusAsyncInvoke()
    .Arguments[0]
    .ShouldBeElementReferenceTo(inputElement);
```

---

## NavigationManager

`BunitNavigationManager` がデフォルトで登録済み。

### ナビゲーション検証

```csharp
var navMan = Services.GetRequiredService<BunitNavigationManager>();

cut.Find("button").Click(); // NavigateTo を呼ぶハンドラ
Assert.AreEqual("http://localhost/target", navMan.Uri);
```

### ナビゲーション履歴

```csharp
var history = navMan.History;
Assert.AreEqual(NavigationState.Succeeded, history[0].NavigationState);
```

### NavigationLock による防止検証

```csharp
var navMan = Services.GetRequiredService<BunitNavigationManager>();
var cut = Render<InterceptComponent>();

cut.Find("button").Click();

var navigationHistory = navMan.History.Single();
Assert.AreEqual(NavigationState.Prevented, navigationHistory.NavigationState);
```

### NavigateToLogin

```csharp
var navMan = Services.GetRequiredService<BunitNavigationManager>();
// ... NavigateToLogin を呼ぶ操作 ...

var requestOptions = navMan.History.Last()
    .StateFromJson<InteractiveRequestOptions>();
Assert.IsNotNull(requestOptions);
Assert.AreEqual(InteractionType.SignIn, requestOptions.Interaction);
```

---

## 認証・認可

`AddAuthorization()` で認証/認可のテストダブルを追加。`<AuthorizeView>` や `<CascadingAuthenticationState>` を使用するコンポーネントのテストに必要。

```csharp
var authContext = AddAuthorization();

// 未認証
authContext.SetNotAuthorized();

// 認証済み
authContext.SetAuthorized("TestUser");

// ロール付き
authContext.SetRoles("Admin", "User");

// クレーム付き
authContext.SetClaims(new Claim(ClaimTypes.Email, "test@example.com"));

// ポリシー付き
authContext.SetPolicies("EditPolicy");
```

---

## HttpClient モック (RichardSzalay.MockHttp)

bUnit には HttpClient の組み込みモックは無い。`RichardSzalay.MockHttp` を使用する。

### ヘルパー拡張メソッド (プロジェクト共通)

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

public static class MockHttpClientBunitHelpers
{
    public static MockHttpMessageHandler AddMockHttpClient(
        this BunitServiceProvider services)
    {
        var mockHttpHandler = new MockHttpMessageHandler();
        var httpClient = mockHttpHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");
        services.AddSingleton(httpClient);
        return mockHttpHandler;
    }

    public static MockedRequest RespondJson<T>(
        this MockedRequest request, T content)
    {
        request.Respond(req =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(
                JsonSerializer.Serialize(content));
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/json");
            return response;
        });
        return request;
    }
}
```

### テストでの使用

```csharp
var mock = Services.AddMockHttpClient();
mock.When("/api/data").RespondJson(new[] { "item1", "item2" });

var cut = Render<DataComponent>();
cut.WaitForAssertion(() =>
    Assert.AreEqual(2, cut.FindAll("li").Count)
);
```

---

## コンポーネントスタブ (ComponentFactories)

子コンポーネントをスタブに置換し、テスト対象コンポーネントを分離する。

```csharp
// 空のスタブに置換
ComponentFactories.AddStub<HeavyChildComponent>();

// カスタムマークアップのスタブ
ComponentFactories.AddStub<HeavyChildComponent>(
    "<div>Stubbed</div>");

var cut = Render<ParentComponent>();
```
