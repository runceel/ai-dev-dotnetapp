# DESIGN: Analytics モジュール

> SPEC: [spec.md](./spec.md) / Issue #22

---

## 1. プロジェクト構成

```
src/Modules/Analytics/
├── EventRegistration.Analytics.Domain/
│   ├── EventRegistration.Analytics.Domain.csproj
│   ├── RegistrationActivity.cs            # アクティビティのルートエンティティ
│   ├── RegistrationActivityType.cs        # enum: Confirmed/WaitListed/Cancelled/PromotedFromWaitList
│   ├── EventStatistics.cs                 # 1 イベント分の集計結果（read-only DTO）
│   └── DailyStatistics.cs                 # 日別集計（DateOnly + 各種カウント）
├── EventRegistration.Analytics.Application/
│   ├── EventRegistration.Analytics.Application.csproj
│   ├── Repositories/
│   │   └── IRegistrationActivityRepository.cs
│   ├── Navigation/
│   │   └── AnalyticsNavigationExtensions.cs
│   └── UseCases/
│       ├── GetEventStatisticsUseCase.cs
│       └── GetDailyStatisticsUseCase.cs
└── EventRegistration.Analytics.Infrastructure/
    ├── EventRegistration.Analytics.Infrastructure.csproj
    ├── Persistence/
    │   ├── AnalyticsDbContext.cs
    │   └── RegistrationActivityRepository.cs
    ├── Handlers/
    │   ├── ParticipantConfirmedAnalyticsHandler.cs
    │   ├── ParticipantWaitListedAnalyticsHandler.cs
    │   ├── RegistrationCancelledAnalyticsHandler.cs
    │   └── ParticipantPromotedFromWaitListAnalyticsHandler.cs
    └── AnalyticsModuleInfrastructureExtensions.cs
```

依存関係:
- `Domain` → 依存なし
- `Application` → `Domain` + `SharedKernel.Application`
- `Infrastructure` → `Domain` + `Application` + `SharedKernel.Application` + `SharedKernel.Infrastructure`

## 2. SharedKernel への追加

新規ドメインイベント 2 件:

```csharp
public sealed record ParticipantWaitListedEvent(
    Guid RegistrationId, Guid EventId,
    string ParticipantName, string ParticipantEmail,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record RegistrationCancelledEvent(
    Guid RegistrationId, Guid EventId,
    RegistrationCancelledPriorStatus PriorStatus,  // Confirmed or WaitListed
    DateTimeOffset OccurredAt) : IDomainEvent;
```

`PriorStatus` を持つことで「Confirmed → Cancelled」と「WaitListed → Cancelled」を Analytics 側で識別できる。`RegistrationStatus` を Domain → SharedKernel に持ち上げると Registrations 寄りの語彙が漏れるため、SharedKernel 側に専用 enum を定義する。

## 3. Registrations モジュールの変更

### `RegisterParticipantUseCase`

`status == WaitListed` で永続化成功した場合、`ParticipantWaitListedEvent` を発行する。

### `CancelRegistrationUseCase`

`registration.Cancel()` 前に保持した `wasConfirmed` を使い、`RegistrationCancelledEvent` を発行する。`PromotedFromWaitListEvent` の発行は既存ロジックを維持。

## 4. Analytics の永続化

`AnalyticsDbContext`:
- `DbSet<RegistrationActivity> Activities`
- InMemory プロバイダー（DB 名 `Analytics`）

`RegistrationActivity`（ルート）:
| フィールド | 型 | 用途 |
|-----------|----|------|
| `Id` | `Guid` | 主キー |
| `EventId` | `Guid` | 対象イベント |
| `RegistrationId` | `Guid` | 対象登録（任意の参考情報） |
| `ActivityType` | `RegistrationActivityType` | enum |
| `OccurredAt` | `DateTimeOffset` | 発生日時 |

## 5. リポジトリ抽象

```csharp
public interface IRegistrationActivityRepository
{
    Task AddAsync(RegistrationActivity activity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<EventStatistics> GetEventStatisticsAsync(Guid eventId, CancellationToken ct = default);
    Task<IReadOnlyList<DailyStatistics>> GetDailyStatisticsAsync(
        Guid eventId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
}
```

集計はリポジトリ実装内で LINQ-to-Entities により行う（InMemory なのでクライアント評価でも問題なし）。

## 6. UseCases

- `GetEventStatisticsUseCase.ExecuteAsync(eventId)` → `EventStatistics`
- `GetDailyStatisticsUseCase.ExecuteAsync(eventId, fromDate, toDate)` → `IReadOnlyList<DailyStatistics>`

## 7. DI 拡張

```csharp
public static IServiceCollection AddAnalyticsModule(this IServiceCollection services)
{
    services.AddSharedKernelDomainEvents();

    services.AddDbContext<AnalyticsDbContext>(o =>
        o.UseInMemoryDatabase("Analytics"));

    services.AddScoped<IRegistrationActivityRepository, RegistrationActivityRepository>();
    services.AddScoped<GetEventStatisticsUseCase>();
    services.AddScoped<GetDailyStatisticsUseCase>();

    services.AddScoped<IDomainEventHandler<ParticipantConfirmedEvent>, ParticipantConfirmedAnalyticsHandler>();
    services.AddScoped<IDomainEventHandler<ParticipantWaitListedEvent>, ParticipantWaitListedAnalyticsHandler>();
    services.AddScoped<IDomainEventHandler<RegistrationCancelledEvent>, RegistrationCancelledAnalyticsHandler>();
    services.AddScoped<IDomainEventHandler<ParticipantPromotedFromWaitListEvent>, ParticipantPromotedFromWaitListAnalyticsHandler>();

    return services;
}
```

## 8. UI

- `Components/Pages/Analytics/AnalyticsList.razor` (`/analytics`): 全イベント一覧 + KPI（参加率・キャンセル率）
- `Components/Pages/Analytics/AnalyticsDetail.razor` (`/analytics/{EventId:guid}`): 統計サマリー + `MudChart`（直近 14 日の日別 Confirmed / Cancelled / WaitListed の Bar）

イベント名解決には `GetAllEventsUseCase` / `GetEventByIdUseCase` を利用（UI 層 = Composition Root のみ）。

## 9. ナビゲーション

```csharp
services.AddSingleton<INavigationItem>(new NavigationItem(
    Title: "統計レポート",
    Href: "/analytics",
    Icon: "Analytics",
    Group: "分析",
    Order: 100,
    Match: NavigationMatch.Prefix));
```

## 10. テスト

- Domain: `RegistrationActivity` 生成テスト、`EventStatistics` 計算テスト
- Application: 各 UseCase の正常系（リポジトリは Fake）
- Infrastructure:
  - 4 つのハンドラがアクティビティを永続化することを `AnalyticsDbContext` で検証
  - `RegistrationActivityRepository` の集計ロジックテスト
  - `AnalyticsModuleInfrastructureExtensions` の DI 解決テスト
- Navigation: `AnalyticsNavigationExtensions` テスト
- Page: bUnit で `AnalyticsList` がイベント一覧を表示すること
