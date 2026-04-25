# SharedKernel モジュール仕様概要

> 対象: イベント参加登録システム — SharedKernel モジュール
> 関連: [architecture.md](../architecture.md)

---

## 概要

複数の業務モジュール（Events / Registrations）から共通で利用されるプリミティブ・抽象型を提供する最下層モジュール。他の業務モジュールには一切依存しない。

---

## データ構造

SharedKernel は **独自の DB（DbContext）を持たない**。
業務モジュール横断で共有されるドメインプリミティブ（値オブジェクト・基底型等）を Domain 層で定義する。

### 将来的な共通型の候補

| 型 | 用途 |
|---|---|
| `Entity<TId>` 基底クラス | 各モジュールのエンティティの共通基底（ID・等価性） |
| `AuditableEntity` | `CreatedAt` / `UpdatedAt` を持つ共通基底 |
| `IResult<T>` | ユースケースの結果型（成功 / 失敗の統一表現） |

---

## 画面

SharedKernel は **独自の画面を持たない**。
UI Shell のナビゲーション抽象（`INavigationItem` / `NavigationItem` / `NavigationMatch`）を `Application` 層で提供し、各業務モジュールが Self-Registration パターンでナビゲーション項目を登録する基盤となる。

### 既存の提供機能（実装済み）

| 機能 | 配置 | 説明 |
|---|---|---|
| `INavigationItem` | `SharedKernel.Application/Navigation` | ナビゲーション項目のインターフェース |
| `NavigationItem` | `SharedKernel.Application/Navigation` | `INavigationItem` の sealed record 実装 |
| `NavigationMatch` | `SharedKernel.Application/Navigation` | ナビゲーションマッチ方式の列挙型 |

---

## ドメインイベント基盤

モジュール間を疎結合に保ったまま副作用（通知等）を波及させるための最小限の抽象を提供する。

### 抽象型 (`SharedKernel.Application/Events/`)

| 型 | 説明 |
|---|---|
| `IDomainEvent` | ドメインイベントのマーカー。`OccurredAt` を持つ。 |
| `IDomainEventHandler<TEvent>` | 単一イベントを購読するハンドラの契約。`HandleAsync(TEvent, CancellationToken)`。 |
| `IDomainEventDispatcher` | 登録された全ハンドラへ配送するディスパッチャの契約。 |

### 既知のイベント契約 (`SharedKernel.Application/Events/`)

クロスモジュールで購読される共通契約は SharedKernel に集約する（CON-008 違反の予防）。

| 型 | 発行元 | 用途 |
|---|---|---|
| `ParticipantConfirmedEvent` | Registrations | 参加が新規に Confirmed 状態で確定した |
| `ParticipantPromotedFromWaitListEvent` | Registrations | キャンセル待ちから繰り上げ確定した |

### 実装 (`SharedKernel.Infrastructure/Events/`)

| 型 | 説明 |
|---|---|
| `ServiceProviderDomainEventDispatcher` | `IServiceProvider` から `IDomainEventHandler<T>` を解決して順次呼び出す既定実装。各ハンドラの例外は `ILogger` に記録され、呼び出し元へ伝播しない。 |
| `SharedKernelEventsServiceCollectionExtensions.AddSharedKernelDomainEvents()` | 上記ディスパッチャを `TryAdd` で登録する DI 拡張。複数モジュールから呼び出しても安全。 |

### 利用方針

- 発行側モジュールは `IDomainEventDispatcher.DispatchAsync` を呼ぶのみ（購読側を知らない）。
- 購読側モジュールは `IDomainEventHandler<T>` を DI に登録するのみ（発行側を知らない）。
- イベント発行は `SaveChangesAsync` 成功後に行うこと（永続化と通知の整合性確保）。
- 購読側の例外は通知失敗としてログに残すに留め、ユースケースの主処理を巻き戻さない。
