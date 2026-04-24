---
name: aspnetcore-blazor10
description: Provides up-to-date information about ASP.NET Core 10 and Blazor .NET 10 new features. Use when the user asks about Blazor .NET 10 capabilities, ASP.NET Core 10 features, OpenAPI, Minimal APIs, SignalR, or needs guidance on ASP.NET Core/Blazor .NET 10 specific features.
---

# ASP.NET Core 10 / Blazor .NET 10 New Features Reference

Source: https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0

---

## Blazor

### Blazor Component Coding Conventions

以下のルールに従ってコンポーネントを実装すること。

#### コードビハインド分離（`@code` ブロック禁止）

`.razor` ファイルに `@code {}` ブロックを記述せず、C# コードは `.razor.cs` ファイルに分離する。コードビハインドクラスは `partial class` として定義する。

```razor
@* Counter.razor — マークアップのみ *@
@page "/counter"

<h1>Counter</h1>
<p>Current count: @currentCount</p>
<button @onclick="IncrementCount">Click me</button>
```

```csharp
// Counter.razor.cs
public partial class Counter
{
    private int currentCount;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

#### DI はコンストラクターインジェクションを使用

`@inject` ディレクティブや `[Inject]` 属性ではなく、コンストラクターインジェクションを使用する。.NET 10 の Blazor コンポーネントではコンストラクターインジェクションがサポートされている。

```csharp
// MyComponent.razor.cs
public partial class MyComponent(IMyService myService, NavigationManager navigationManager)
{
    protected override async Task OnInitializedAsync()
    {
        var data = await myService.GetDataAsync();
    }
}
```

#### コンポーネント固有の CSS は `.razor.css` に分離

コンポーネントに固有のスタイルはコンポーネントと同名の `.razor.css` ファイルに定義する（CSS isolation）。

```css
/* Counter.razor.css */
h1 {
    color: #333;
}

button {
    background-color: #0078d4;
    color: white;
}
```

---

### QuickGrid Enhancements

**`RowClass` parameter** — Apply a CSS class to a row based on row item:

```razor
<QuickGrid Items="movies" RowClass="GetRowCssClass">
    <PropertyColumn Property="@(m => m.Title)" />
</QuickGrid>

@code {
    private string? GetRowCssClass(Movie item) =>
        item.IsArchived ? "row-archived" : null;
}
```

**`HideColumnOptionsAsync`** — Close column options UI programmatically:

```razor
<QuickGrid @ref="grid" Items="items">
    <PropertyColumn Property="@(m => m.Title)">
        <ColumnOptions>
            <input @bind="filter" @bind:after="@(() => grid.HideColumnOptionsAsync())" />
        </ColumnOptions>
    </PropertyColumn>
</QuickGrid>
```

---

### `[PersistentState]` — Declarative Prerendering State Persistence

Before (.NET 9):

```razor
@implements IDisposable
@inject PersistentComponentState ApplicationState

@code {
    public List<Movie>? MoviesList { get; set; }
    private PersistingComponentStateSubscription? sub;

    protected override async Task OnInitializedAsync()
    {
        if (!ApplicationState.TryTakeFromJson<List<Movie>>(nameof(MoviesList), out var movies))
            MoviesList = await MovieService.GetMoviesAsync();
        else
            MoviesList = movies;

        sub = ApplicationState.RegisterOnPersisting(() =>
        {
            ApplicationState.PersistAsJson(nameof(MoviesList), MoviesList);
            return Task.CompletedTask;
        });
    }

    public void Dispose() => sub?.Dispose();
}
```

After (.NET 10):

```razor
@code {
    [PersistentState]
    public List<Movie>? MoviesList { get; set; }

    protected override async Task OnInitializedAsync()
    {
        MoviesList ??= await MovieService.GetMoviesAsync();
    }
}
```

**`AllowUpdates`** — opt-in to update state during enhanced navigation:

```csharp
[PersistentState(AllowUpdates = true)]
public WeatherForecast[]? Forecasts { get; set; }
```

**`RestoreBehavior`** options:

```csharp
[PersistentState(RestoreBehavior = RestoreBehavior.SkipInitialValue)]
public string NoPrerenderedData { get; set; }

[PersistentState(RestoreBehavior = RestoreBehavior.SkipLastSnapshot)]
public int CounterNotRestoredOnReconnect { get; set; }
```

**Custom serializer for persistent state:**

```csharp
// Program.cs
builder.Services.AddSingleton<PersistentComponentStateSerializer<TUser>, CustomUserSerializer>();
```

---

### Improved Form Validation

Validates nested objects and collection items. Uses source generator (AOT-compatible) instead of reflection.

**Setup:**

```csharp
// Program.cs
builder.Services.AddValidation();
```

**Model** (must be a `.cs` file, not inline in `.razor`):

```csharp
[ValidatableType]  // required on root type only
public class Order
{
    public Customer Customer { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = [];
}

public class Customer
{
    [Required(ErrorMessage = "Name is required.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string? Email { get; set; }
}
```

**Component** — uses `DataAnnotationsValidator` as before:

```razor
<EditForm Model="Model">
    <DataAnnotationsValidator />
    <InputText @bind-Value="Model!.Customer.FullName" />
    <ValidationMessage For="@(() => Model!.Customer.FullName)" />
</EditForm>
```

**Validation rules** (all applied in order; short-circuits on first error):
1. Member property attributes (including nested objects)
2. Type-level attributes
3. `IValidatableObject.Validate`

`[SkipValidation]` to exclude specific properties or types from validation.

---

### New JavaScript Interop APIs

**Create a JS object instance:**

```csharp
// Async
var objRef = await JSRuntime.InvokeConstructorAsync("MyLib.MyClass", arg1, arg2);
var value = await objRef.GetValueAsync<string>("propertyName");
await objRef.SetValueAsync("propertyName", newValue);

// Sync (in-process)
var inProc = (IJSInProcessRuntime)JSRuntime;
var objRef = inProc.InvokeConstructor("MyLib.MyClass", arg1);
var value = objRef.GetValue<string>("propertyName");
inProc.SetValue("propertyName", newValue);
```

**Read/write JS object properties:**

```csharp
// Read
var num = await JSRuntime.GetValueAsync<int>("myLib.config.retryCount");

// Write
await JSRuntime.SetValueAsync("myLib.config.retryCount", 5);
```

---

### Not Found Handling

**`Router.NotFoundPage` parameter** — specify a page for 404:

```razor
<Router AppAssembly="@typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <!-- <NotFound> is ignored when NotFoundPage is defined -->
</Router>
```

**`NavigationManager.NotFound()`** — trigger Not Found programmatically:

```csharp
@inject NavigationManager Nav

protected override async Task OnInitializedAsync()
{
    var item = await Service.GetItemAsync(Id);
    if (item is null)
        Nav.NotFound();
}
```

Behavior:
- Static SSR: sets HTTP 404
- Interactive rendering: renders the `NotFoundPage`
- Streaming rendering: renders `NotFoundPage` via enhanced navigation

Subscribe to notifications: `NavigationManager.OnNotFound`.

---

### Passkey (WebAuthn/FIDO2) Support for ASP.NET Core Identity

Password-less login using biometrics or security keys. Built into the Blazor Web App project template.

```csharp
// Program.cs — already included in the new project template
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

See: https://learn.microsoft.com/aspnet/core/security/authentication/passkeys

---

### Circuit State Persistence

Server-side Blazor (SignalR) sessions survive connection loss without losing unsaved work:
- Browser tab throttling
- Mobile app switching
- Network interruptions
- Proactive pausing of inactive circuits
- Enhanced navigation

See: https://learn.microsoft.com/aspnet/core/blazor/state-management/server

---

### Hot Reload for Blazor WebAssembly

```xml
<!-- Enabled by default for Debug configuration -->
<WasmEnableHotReload>true</WasmEnableHotReload>
```

---

### Navigation Changes

- **`NavigateTo` same-page**: No longer scrolls to top (viewport preserved on query string / fragment changes)
- **`NavLink` with `NavLinkMatch.All`**: Ignores query string and fragment — link stays `active` on path match

  Revert: set `AppContext` switch `Microsoft.AspNetCore.Components.Routing.NavLink.EnableMatchAllForQueryStringAndFragment` to `true`

  Override matching:
  ```csharp
  public class CustomNavLink : NavLink
  {
      protected override bool ShouldMatch(string currentUriAbsolute) { ... }
  }
  ```

---

### Disable `NavigationException` during Static SSR

```xml
<!-- Program.cs template sets this to true by default in .NET 10 -->
<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>
```

With this set, `NavigationManager.NavigateTo` during static SSR no longer throws `NavigationException`. Code after `NavigateTo` executes before the redirect occurs.

---

### Reconnection UI (`ReconnectModal`)

Template now includes a `ReconnectModal` component with collocated CSS/JS. CSP-compliant (no inline styles).

New event: `components-reconnect-state-changed`  
New state: `"retrying"` (in addition to existing states)

---

### `OwningComponentBase` implements `IAsyncDisposable`

```csharp
// DisposeAsync and DisposeAsyncCore now available
public class MyComponent : OwningComponentBase
{
    // async cleanup supported
}
```

---

### New `InputHidden` Component

```razor
<EditForm Model="Parameter" OnValidSubmit="Submit" FormName="Example">
    <InputHidden id="hidden" @bind-Value="Parameter" />
    <button type="submit">Submit</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    public string Parameter { get; set; } = "stranger";

    private void Submit() { }
}
```

---

### Client-Side Fingerprinting (Standalone Blazor WebAssembly)

```xml
<!-- .csproj -->
<OverrideHtmlAssetPlaceholders>true</OverrideHtmlAssetPlaceholders>
```

```html
<!-- wwwroot/index.html -->
<head>
    <script type="importmap"></script>
</head>
<body>
    <script src="_framework/blazor.webassembly#[.{fingerprint}].js"></script>
</body>
```

Fingerprint JS modules:
```html
<script type="module" src="js/scripts#[.{fingerprint}].js"></script>
```

```xml
<ItemGroup>
  <StaticWebAssetFingerprintPattern Include="JSModule" Pattern="*.js" Expression="#[.{fingerprint}]!" />
</ItemGroup>
```

---

### WebAssembly Environment Configuration

```xml
<!-- No longer uses launchSettings.json -->
<WasmApplicationEnvironmentName>Staging</WasmApplicationEnvironmentName>
```

Defaults: `Development` (build), `Production` (publish).

---

### `HttpClient` Response Streaming Enabled by Default

`response.Content.ReadAsStreamAsync()` now returns `BrowserHttpReadStream` (not `MemoryStream`). Synchronous `Stream.Read` is not supported.

Opt out globally:

```xml
<WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>
```

Opt out per request:

```csharp
requestMessage.SetBrowserResponseStreamingEnabled(false);
```

---

### Static Asset Preloading

**Blazor Web Apps** — use `<ResourcePreloader />` component:

```razor
<!-- App.razor -->
<head>
    <base href="/" />
    <ResourcePreloader />
</head>
```

**Standalone Blazor WebAssembly** — add `<link rel="preload" id="webassembly" />` in `wwwroot/index.html` (requires `OverrideHtmlAssetPlaceholders`).

---

### Blazor Script as Static Web Asset

`blazor.web.js` / `blazor.server.js` now served as a static web asset with automatic compression and fingerprinting.

If your app has no `.razor` files but needs the Blazor script:
```xml
<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>
```

---

### `BlazorCacheBootResources` Removed

```diff
- <BlazorCacheBootResources>...</BlazorCacheBootResources>
```

All Blazor client-side files are now automatically fingerprinted and browser-cached.

---

### Metrics and Tracing

Comprehensive observability: component lifecycle, navigation, event handling, circuit management.

See: https://learn.microsoft.com/aspnet/core/blazor/performance

---

### JavaScript Bundler Support

```xml
<WasmBundlerFriendlyBootConfig>true</WasmBundlerFriendlyBootConfig>
```

Produces bundler-friendly output (Gulp, Webpack, Rollup) during publish.

---

### PWA Service Worker Caching Fix

```diff
- navigator.serviceWorker.register('service-worker.js');
+ navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });
```

---

## Minimal APIs

### Built-In Validation

```csharp
// Program.cs
builder.Services.AddValidation();
```

Validates query, header, and request body using `DataAnnotations` attributes. Returns 400 with validation errors.

Disable per endpoint:
```csharp
app.MapPost("/products", handler).DisableValidation();
```

Validation with record types:
```csharp
public record Product(
    [Required] string Name,
    [Range(1, 1000)] int Quantity);

app.MapPost("/products", (Product product) => TypedResults.Ok(product));
```

Customize error responses via `IProblemDetailsService`.

---

### Server-Sent Events (SSE)

```csharp
app.MapGet("/events", (CancellationToken ct) =>
{
    async IAsyncEnumerable<HeartRateRecord> Stream(
        [EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            yield return HeartRateRecord.Create(Random.Shared.Next(60, 100));
            await Task.Delay(2000, ct);
        }
    }
    return TypedResults.ServerSentEvents(Stream(ct), eventType: "heartRate");
});
```

Supported in both Minimal APIs and controller-based apps.

---

### Empty String in Form Post as `null`

When using `[FromForm]` with complex objects, empty strings are now treated as `null` for nullable types (no parse failure).

```csharp
app.MapPost("/todo", ([FromForm] Todo todo) => TypedResults.Ok(todo));

public class Todo
{
    public DateOnly? DueDate { get; set; }  // empty string → null
}
```

---

## OpenAPI

### OpenAPI 3.1 (Default)

Default OpenAPI version is now **3.1** (was 3.0). Notable schema changes:
- Nullable types: no `nullable: true`; instead `"type": ["string", "null"]`
- `int`/`long` with `AllowReadingFromString`: no `type: integer`, uses `pattern`

Override version:
```csharp
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});
```

Build-time:
```xml
<OpenApiGenerateDocumentsOptions>--openapi-version OpenApi3_0</OpenApiGenerateDocumentsOptions>
```

**Breaking change**: `OpenApiAny` replaced with `JsonNode`:

```diff
options.AddSchemaTransformer((schema, context, ct) =>
{
    if (context.JsonTypeInfo.Type == typeof(WeatherForecast))
    {
-       schema.Example = new OpenApiObject
+       schema.Example = new JsonObject
        {
-           ["date"] = new OpenApiString("2025-01-01"),
+           ["date"] = "2025-01-01",
-           ["temperatureC"] = new OpenApiInteger(0),
+           ["temperatureC"] = 0,
        };
    }
    return Task.CompletedTask;
});
```

---

### OpenAPI in YAML

```csharp
app.MapOpenApi("/openapi/{documentName}.yaml");  // .yaml or .yml suffix
```

---

### Response Description on `ProducesResponseType`

```csharp
[HttpGet]
[ProducesResponseType<IEnumerable<WeatherForecast>>(
    StatusCodes.Status200OK,
    Description = "The weather forecast for the next 5 days.")]
public IEnumerable<WeatherForecast> Get() { ... }
```

Works for both API controllers and Minimal APIs.

---

### XML Doc Comments in OpenAPI

Enable in `.csproj`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

XML comments on methods, classes, and members are included in the generated OpenAPI document.

Cross-assembly XML comments also supported via project file configuration.

---

### Schema Generation in Transformers

```csharp
options.AddDocumentTransformer(async (document, context, ct) =>
{
    var schema = await context.GetOrCreateSchemaAsync(typeof(MyType));
    document.AddComponent("MyType", schema);
});
```

---

### Form Data Enum Parameters

Form data enum parameters now use the actual enum type in OpenAPI metadata (not `string`).

---

## Authentication and Authorization

- Authentication and authorization metrics
- ASP.NET Core Identity metrics
- Avoid cookie login redirects for known API endpoints (API requests no longer redirect to login page)

---

## Miscellaneous

### Automatic Memory Pool Eviction

Memory pools evict unused memory automatically. Monitor via metrics.

### `System.Text.Json`-Based JSON Patch

New `JsonPatchDocument` implementation using `System.Text.Json` (replaces Newtonsoft.Json dependency):

```csharp
var patch = new JsonPatchDocument<MyModel>();
patch.Replace(m => m.Name, "New Name");
patch.Add(m => m.Tags, "new-tag");
patch.ApplyTo(myModel);
```

### Detect Local URL

```csharp
if (RedirectHttpResult.IsLocalUrl(returnUrl))
    return Redirect(returnUrl);
```

### `Json+PipeReader` Deserialization

MVC and Minimal APIs now support `PipeReader`-based JSON deserialization for improved efficiency.
