# Implementation Plan: デモ用シードデータの自動投入機能 (Issue #24)

## アーキテクチャ概要

```
EventRegistration.Web (Composition Root)
└─ DemoDataHostedService (IHostedService, StartAsync で実行)
   ├─ DemoDataOptions { bool Enabled }  ← appsettings から bind
   └─ IEnumerable<IDemoDataSeeder> (SharedKernel.Application 抽象)
        ├─ EventsDemoDataSeeder        (Events.Infrastructure, Order=10)
        └─ RegistrationsDemoDataSeeder (Registrations.Infrastructure, Order=20)
```

各モジュールは自分の `Infrastructure` 内で `IDemoDataSeeder` を実装し、
`AddXxxModuleInfrastructure()` で DI 登録する。Web は両者を `IEnumerable` として列挙し、
`Order` 昇順で実行する。

## 詳細

### 1. SharedKernel.Application/DemoData/IDemoDataSeeder.cs
```csharp
public interface IDemoDataSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken);
}
```

### 2. EventsDemoDataSeeder
- `Order = 10`
- `EventsDbContext.Events.AnyAsync()` が `true` ならスキップ
- 3 件 (`Event.Create(...)` を使用) を投入

### 3. RegistrationsDemoDataSeeder
- `Order = 20`
- `RegistrationsDbContext.Registrations.AnyAsync()` が `true` ならスキップ
- `EventsDbContext` から既存イベント ID を取得し、Confirmed/WaitListed の参加者を投入
- イベントが空の場合は何もしない (Events シーダーが OFF/失敗のとき)

### 4. DemoDataOptions
```csharp
public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";
    public bool Enabled { get; set; }
}
```

### 5. DemoDataHostedService
- `IServiceScopeFactory` から scope を作成し、`IDemoDataSeeder` を `Order` 順に実行
- `Options.Enabled == false` ならスキップ
- 例外発生時は `ILogger` で警告ログを残し、起動は継続

### 6. Program.cs
```csharp
builder.Services
    .AddOptions<DemoDataOptions>()
    .Bind(builder.Configuration.GetSection(DemoDataOptions.SectionName))
    .PostConfigure(o =>
    {
        // 設定が空のとき、Development だけ既定で ON
        if (!builder.Configuration.GetSection(DemoDataOptions.SectionName).Exists())
        {
            o.Enabled = builder.Environment.IsDevelopment();
        }
    });

builder.Services.AddHostedService<DemoDataHostedService>();
```

### 7. appsettings.Development.json
- `"DemoData": { "Enabled": true }` を明示

### 8. テスト
- `EventsDemoDataSeeder_Seeds_When_Empty`
- `EventsDemoDataSeeder_Skips_When_NotEmpty`
- `RegistrationsDemoDataSeeder_Seeds_When_Empty`
- `RegistrationsDemoDataSeeder_Skips_When_NoEvents`
- `DemoDataHostedService_DoesNotSeed_When_Disabled`
- `DemoDataHostedService_Runs_Seeders_In_Order`
