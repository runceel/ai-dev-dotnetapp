# PR Body — Analytics（統計・レポート）モジュール追加

> Closes #22

## 概要

Issue #22 の要件に従い、CQRS の Read 側として `Analytics` モジュールを物理 3 分割で追加した。`Events` / `Registrations` モジュールへ直接参照せず、`SharedKernel.Application.Events` のドメインイベントを購読することでモジュール境界を保ったまま統計データを構築する。

## 主な変更

### 新規モジュール

```
src/Modules/Analytics/
├── EventRegistration.Analytics.Domain/         # RegistrationActivity / RegistrationActivityType / EventStatistics / DailyStatistics
├── EventRegistration.Analytics.Application/    # IRegistrationActivityRepository / 2 つの UseCase / Navigation 拡張
└── EventRegistration.Analytics.Infrastructure/ # AnalyticsDbContext / Repository / 4 つの DomainEventHandler / DI 拡張
```

依存方向: `Analytics.* → SharedKernel.*` のみ。

### SharedKernel ドメインイベント追加

- `ParticipantWaitListedEvent`
- `RegistrationCancelledEvent`（`RegistrationCancelledPriorStatus` を含む）

### Registrations の更新

- `RegisterParticipantUseCase`: `WaitListed` 確定時に `ParticipantWaitListedEvent` を発行
- `CancelRegistrationUseCase`: `RegistrationCancelledEvent` を発行（既存の繰り上げイベントは維持）

### UI

- `/analytics`：全イベントの KPI 一覧（参加率・キャンセル率を含む）
- `/analytics/{eventId:guid}`：イベント詳細サマリー + `MudChart` (Bar) による直近 14 日推移
- ナビゲーション「分析 > 統計レポート」（アイコン: `Analytics`）

### ドキュメント

- `docs/architecture.md` §2.3 に Notifications / Analytics モジュールの位置づけを追記

## モジュール境界

Analytics モジュールは下記のみを参照する:

- `SharedKernel.Application`（`IDomainEvent` / `IDomainEventHandler<T>` / `INavigationItem`）
- `SharedKernel.Infrastructure`（`AddSharedKernelDomainEvents`）

`Events` / `Registrations` / `Notifications` への直接参照は **なし**。データ連携は `IDomainEventDispatcher` 経由のドメインイベントで完結。

## テスト

- 既存: 124 passed
- 追加 / 更新後: **146 passed** (Failed: 0, Skipped: 0)

追加カテゴリ:
- `Modules/Analytics/Domain/EventStatisticsTests` – 集計プロパティ
- `Modules/Analytics/Application/AnalyticsUseCaseTests` – UseCase 委譲・引数検証
- `Modules/Analytics/Infrastructure/AnalyticsHandlersAndRepositoryTests` – 4 ハンドラ + Repository 集計 / 日別集計 / DI 解決
- `Modules/Analytics/Navigation/AnalyticsNavigationExtensionsTests` – ナビ項目登録
- `Components/Pages/Analytics/AnalyticsListTests` – bUnit によるページレンダリング
- `Integration/AnalyticsCrossModuleIntegrationTests` – Registrations → Analytics の通し検証

更新:
- `Modules/Registrations/Application/RegistrationsDomainEventPublicationTests` – 新イベント発行に追従

## 配線

`Program.cs` への変更は 2 行のみ:

```csharp
builder.Services.AddAnalyticsModuleNavigation();
// ...
builder.Services.AddAnalyticsModule();
```

## スモーク確認

`dotnet run` により `/analytics` を取得した結果:

- HTTP 200 を確認
- レスポンス HTML に `<title>統計レポート</title>` が出力される
- ナビゲーションメニューに「統計レポート」が表示される

## ライフサイクル成果物

`cloud-agent-lifecycle` スキルに従い、ライフサイクル関連成果物を `.github/lifecycle/issue-22/` 配下にファイルとして保存:

- `state.yaml` / `spec.md` / `design.md` / `implementation-plan.md` / `pull-request.md` / `log.md`
