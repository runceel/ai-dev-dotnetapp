---
name: dotnet10
description: Provides up-to-date information about .NET 10 and C# 14 new features for C# developers. Use when the user asks about .NET 10 capabilities, C# 14 language features, ASP.NET Core 10 Blazor, EF Core 10, or needs guidance on .NET 10/C# 14 specific features.
---

# .NET 10 & C# 14 Reference for C# Developers

Source: https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview

.NET 10 is an **LTS (Long-Term Support)** release. C# 14 ships with .NET 10.

---

## C# 14

Reference: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14

### Extension Members (`extension` blocks)

New syntax for extension properties, static extension members, and user-defined operators as extensions:

```csharp
public static class MyExtensions
{
    // Instance extension members
    extension<TSource>(IEnumerable<TSource> source)
    {
        public bool IsEmpty => !source.Any();
        public IEnumerable<TSource> Where(Func<TSource, bool> predicate) { ... }
    }

    // Static extension members and operators
    extension<TSource>(IEnumerable<TSource>)
    {
        public static IEnumerable<TSource> Empty => Enumerable.Empty<TSource>();
        public static IEnumerable<TSource> operator +(IEnumerable<TSource> left, IEnumerable<TSource> right)
            => left.Concat(right);
    }
}

// Usage
var empty = sequence.IsEmpty;          // instance extension property
var id = IEnumerable<int>.Empty;       // static extension property
var combined = list1 + list2;          // extension operator
```

### Field-Backed Properties (`field` keyword)

Access the compiler-generated backing field using `field`:

```csharp
// Before C# 14
private string _name;
public string Name
{
    get => _name;
    set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
}

// C# 14
public string Name
{
    get;
    set => field = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
}
```

Use `@field` or `this.field` to disambiguate if a member named `field` exists.

### Implicit `Span<T>` / `ReadOnlySpan<T>` Conversions

First-class support for `Span<T>` and `ReadOnlySpan<T>`:
- New implicit conversions between `Span<T>`, `ReadOnlySpan<T>`, and `T[]`
- Span types can be extension method receivers
- Compose with other conversions and improve generic type inference

### `nameof` with Unbound Generic Types

```csharp
nameof(List<>)         // "List"
nameof(Dictionary<,>)  // "Dictionary"
```

### Lambda Parameter Modifiers Without Types

```csharp
delegate bool TryParse<T>(string text, out T result);

// C# 14: modifiers without type annotation
TryParse<int> parse = (text, out result) => Int32.TryParse(text, out result);

// Before C# 14: types required
TryParse<int> parse = (string text, out int result) => Int32.TryParse(text, out result);
```

Supported modifiers: `scoped`, `ref`, `in`, `out`, `ref readonly`. (`params` still requires typed parameters.)

### Partial Constructors and Partial Events

```csharp
// Partial constructor
public partial class MyService
{
    public partial MyService(ILogger<MyService> logger);  // defining declaration
}
public partial class MyService
{
    private readonly ILogger<MyService> _logger;
    public partial MyService(ILogger<MyService> logger)   // implementing declaration
    {
        _logger = logger;
    }
}

// Partial event
public partial class MyComponent
{
    public partial event EventHandler Clicked;  // defining (field-like)
}
public partial class MyComponent
{
    public partial event EventHandler Clicked   // implementing
    {
        add => _clicked += value;
        remove => _clicked -= value;
    }
}
```

### Null-Conditional Assignment

```csharp
// Before C# 14
if (customer is not null)
    customer.Order = GetCurrentOrder();

// C# 14
customer?.Order = GetCurrentOrder();   // right side evaluated only when left is not null
customer?.Total += discount;           // compound assignment also supported
// Note: ++ and -- are NOT allowed
```

### User-Defined Compound Assignment and Increment/Decrement Operators

```csharp
public static MyVector operator +=(MyVector left, MyVector right)
    => new(left.X + right.X, left.Y + right.Y);

public static MyCounter operator ++(MyCounter c) => new(c.Value + 1);
public static MyCounter operator --(MyCounter c) => new(c.Value - 1);
```

---

## .NET Libraries (New APIs)

Reference: https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/libraries

### JSON Serialization (`System.Text.Json`)
- Disallow duplicate properties option
- Strict serialization settings
- `PipeReader` support for stream-based deserialization

### Post-Quantum Cryptography
- Windows CNG support
- ML-DSA: simplified APIs, HashML-DSA, Composite ML-DSA

### Cryptography
- AES KeyWrap with Padding (`AesKeyWrapPad`)

### Networking
- `WebSocketStream`: wraps `WebSocket` as a `Stream`
- TLS 1.3 support for macOS clients

### Process Management
- Windows process group support (`CreateJobObject`-based signal isolation)

---

## .NET SDK

- **`dotnet test`** supports Microsoft.Testing.Platform
- **One-shot tool execution**: `dotnet tool exec <tool>` — run without installing globally
- **`dnx` script**: shorthand for `dotnet tool exec` (`dnx dotnetsay "Hello"`)
- Console apps can **natively create container images** (no Docker required)
- Configurable container image format via MSBuild property
- Platform-specific .NET tools (self-contained, trimmed, AOT per RID in one package)

---

## ASP.NET Core 10 / Blazor

Reference: https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0

### Identity / Security
- **Passkey (WebAuthn/FIDO2)** support in ASP.NET Core Identity — password-less login, built into the Blazor Web App template

### Blazor
- **`[PersistentState]` attribute**: Declarative prerendering state persistence for components and services
  ```csharp
  [PersistentState]
  private MyData? Data { get; set; }
  ```
- **Improved form validation**: Validates nested object properties and collection items
  - Opt in: `AddValidation()` in DI + `[ValidatableType]` on root model
- **Circuit state persistence**: Server-side Blazor session survives connection loss
- **Hot Reload for Blazor WebAssembly**: `WasmEnableHotReload` MSBuild property (default `true` for Debug)
- **New JS interop APIs**:
  - Create JS object instances via constructor function
  - Read/modify JS object property values
- **`<ResourcePreloader />`** component for WebAssembly asset preloading
- **`QuickGrid` `RowClass` parameter**: Apply CSS class per row based on row item
- `NavigateTo` no longer scrolls to top on same-page navigation
- `NavLink` ignores query string/fragment with `NavLinkMatch.All`
- `BlazorDisableThrowNavigationException` MSBuild property to suppress `NavigationException` during static SSR

### OpenAPI
- Form data enum parameters use actual enum type (not `string`)
- `GetOrCreateSchemaAsync` in transformers to generate `OpenApiSchema` from C# types
- Endpoint-specific operation transformers
- Cross-assembly XML doc comment support

---

## EF Core 10

Reference: https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew

- LINQ enhancements (more queries translate to SQL)
- Performance optimizations
- Improved Azure Cosmos DB support
- **Named query filters**: Assign names to query filters; selectively disable per query
  ```csharp
  // Configuration
  modelBuilder.Entity<Post>().HasQueryFilter("SoftDelete", p => !p.IsDeleted);
  modelBuilder.Entity<Post>().HasQueryFilter("TenantFilter", p => p.TenantId == currentTenantId);

  // Selectively ignore
  context.Posts.IgnoreQueryFilters("SoftDelete").ToList();
  ```
