# SPEC: Analytics（統計・レポート）モジュール追加

> **Issue**: #22
> **Branch**: `copilot/add-analytics-module-again`
> **Transport**: `repo-files`（cloud-agent-lifecycle）

---

## 1. 目的・背景

イベント参加登録システムの運営改善に向け、各イベントの参加状況・キャンセル状況を集計・可視化するための **Analytics モジュール** を追加する。Modular Monolith × Clean Architecture の既存方針に従い、他モジュールへ直接依存せず、ドメインイベントを購読して構築した自モジュール内の Read モデル経由で統計を提供する。

## 2. スコープ

### 含むもの

- `src/Modules/Analytics/` 配下に物理 3 分割で新モジュールを追加
  - `EventRegistration.Analytics.Domain`
  - `EventRegistration.Analytics.Application`
  - `EventRegistration.Analytics.Infrastructure`
- 既存パターン（Notifications モジュール）に倣ったドメインイベント購読
- 必要なドメインイベントを `SharedKernel.Application.Events` に追加
- Registrations モジュールの UseCase から新規イベントを発行
- `EventRegistration.Web` に Analytics ページを追加し、`MudChart` で可視化
- DI / Navigation の Self-Registration

### 含まないもの

- 認可・アクセス制御（管理者限定 UI）
- 永続化バックエンド変更（既存どおり EF Core InMemory）
- Excel / CSV エクスポート、外部 BI 連携
- リアルタイム集計（pull 型ページ更新で十分）

## 3. 機能要件

### FR-01 集計対象アクティビティ

Analytics モジュールは以下のアクティビティを記録する:

| アクティビティ種別 | 発火元 |
|---|---|
| `Confirmed` | 新規登録が確定した時 |
| `WaitListed` | 新規登録がキャンセル待ちになった時（**新規追加イベント**） |
| `Cancelled` | 登録がキャンセルされた時（**新規追加イベント**） |
| `PromotedFromWaitList` | キャンセル待ちから繰り上がり確定した時 |

### FR-02 イベント別統計

`GetEventStatisticsUseCase` は指定イベント ID について以下を返す:

- `EventId`
- `ConfirmedCount`：初期登録時 `Confirmed` 件数
- `WaitListedCount`：初期登録時 `WaitListed` 件数
- `CancelledCount`：キャンセル件数
- `PromotedCount`：繰り上がり件数
- `TotalRegistrations` = `ConfirmedCount + WaitListedCount`
- `FinalConfirmedCount` = `ConfirmedCount + PromotedCount - <キャンセル時に Confirmed だった件数>`
  - 第一版では実装簡略化のため `ConfirmedCount + PromotedCount - CancelledCount`（負値の場合は 0）
- `ParticipationRate`：`TotalRegistrations > 0` のとき `FinalConfirmedCount / TotalRegistrations`、それ以外は `0`

### FR-03 期間集計

`GetDailyStatisticsUseCase(eventId, fromDateUtc, toDateUtc)` は指定日範囲について日別集計を返す:

- `Date`（`DateOnly`、UTC）
- `ConfirmedCount` / `WaitListedCount` / `CancelledCount` / `PromotedCount`

### FR-04 ナビゲーション

- 「Analytics」グループに「統計レポート」項目を追加（Href `/analytics`、アイコン `Analytics`）
- 既存 `IconResolver` には `Analytics` キーが既にマッピング済み

### FR-05 UI 要件

- 一覧ページ `/analytics`：登録済みイベント一覧と各イベントの主要 KPI（参加率・キャンセル率）を表示
- 詳細ページ `/analytics/{eventId:guid}`：選択イベントの統計サマリー + `MudChart`（種類: Bar）で日別推移を表示

## 4. 非機能要件

| 項目 | 要件 |
|------|------|
| 言語/ランタイム | C# 14 / .NET 10 |
| データベース | EF Core 10 InMemory（既存方針踏襲） |
| 依存関係 | Analytics → SharedKernel のみ。Events / Registrations / Notifications モジュールへ直接参照しない |
| エラー伝播 | ドメインイベントハンドラ内の例外は既存 `ServiceProviderDomainEventDispatcher` がログに記録し主処理を巻き戻さない |

## 5. 受け入れ基準（Acceptance Criteria）

- **AC-01**: 新規 3 プロジェクトが `EventRegistration.slnx` に追加され `dotnet build` が成功する
- **AC-02**: Analytics プロジェクトの依存先は `SharedKernel.*` のみ（プロジェクト参照で検証可能）
- **AC-03**: 新規登録で `Confirmed` または `WaitListed` のいずれかのイベントが Analytics に記録される
- **AC-04**: キャンセル時に `Cancelled` イベントが記録され、繰り上がり発生時は `PromotedFromWaitList` も記録される
- **AC-05**: `GetEventStatisticsUseCase` がイベントごとの集計値を返す
- **AC-06**: `GetDailyStatisticsUseCase` が指定日範囲（含む側）に該当する日別集計を返す
- **AC-07**: `Program.cs` 1 行 (`AddAnalyticsModule()` + `AddAnalyticsModuleNavigation()`) で配線完結
- **AC-08**: `/analytics` および `/analytics/{eventId}` が表示される（bUnit テストで最低限のレンダリングを検証）
- **AC-09**: 全テストが PASS する

## 6. 影響範囲

- **新規追加**: `src/Modules/Analytics/**`
- **変更**: `EventRegistration.slnx` / `EventRegistration.Web.csproj` / `Program.cs` / `Components/_Imports.razor` / `SharedKernel.Application/Events/*` / `Registrations.Application/UseCases/*`
- **テスト追加**: `tests/.../Modules/Analytics/**` / `tests/.../Components/Pages/Analytics/**`
- **ドキュメント**: `docs/architecture.md` の §1 ディレクトリ構成に Analytics モジュールを追記
