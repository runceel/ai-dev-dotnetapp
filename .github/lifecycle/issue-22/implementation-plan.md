# IMPLEMENTATION PLAN: Issue #22 — Analytics モジュール

> SPEC: [spec.md](./spec.md) / DESIGN: [design.md](./design.md)

## ステップ概要

| # | 対象 | 内容 |
|---|------|------|
| 1 | SharedKernel | `ParticipantWaitListedEvent` / `RegistrationCancelledEvent` (+ `RegistrationCancelledPriorStatus`) を追加 |
| 2 | Registrations | `RegisterParticipantUseCase` で WaitListed 時に新イベントを発行 |
| 3 | Registrations | `CancelRegistrationUseCase` で `RegistrationCancelledEvent` を発行 |
| 4 | Analytics.Domain | `RegistrationActivity` / `RegistrationActivityType` / `EventStatistics` / `DailyStatistics` を追加 |
| 5 | Analytics.Application | `IRegistrationActivityRepository` / `GetEventStatisticsUseCase` / `GetDailyStatisticsUseCase` / `AnalyticsNavigationExtensions` を追加 |
| 6 | Analytics.Infrastructure | `AnalyticsDbContext` / `RegistrationActivityRepository` / 4 つのドメインイベントハンドラ / `AnalyticsModuleInfrastructureExtensions` を追加 |
| 7 | Solution | `EventRegistration.slnx` に新規 3 プロジェクトを追加 |
| 8 | Web | `EventRegistration.Web.csproj` に Analytics の参照を追加し、`Program.cs` で配線 |
| 9 | UI | `Components/Pages/Analytics/AnalyticsList.razor` / `AnalyticsDetail.razor` を作成 |
| 10 | テスト | Domain / Application / Infrastructure / Navigation / bUnit / 通し統合テストを追加 |
| 11 | 既存テスト | 新イベント発行に追従して `RegistrationsDomainEventPublicationTests` を更新 |
| 12 | ドキュメント | `docs/architecture.md` の §2.3 にモジュール説明を追記 |

## 検証

- ビルド: `dotnet build EventRegistration.slnx` → 0 errors
- テスト: `dotnet test EventRegistration.slnx` → 146 passed (元 124 → +22)
- スモーク: `dotnet run` で `/analytics` が HTTP 200 を返し、ナビゲーションに「統計レポート」項目が表示されることを確認
