# Notifications モジュール仕様概要

> 対象: イベント参加登録システム — Notifications モジュール
> 関連: [event-registration-system-spec.md](../event-registration-system-spec.md) / [architecture.md](../architecture.md) / [shared-kernel.md](./shared-kernel.md) / [registrations.md](./registrations.md)

---

## 概要

参加登録に関する重要な状態遷移（参加確定・キャンセル待ち繰り上げ）を購読し、
通知（現状はログ出力のみ）を行う Bounded Context。

`EventRegistration.Notifications.Domain` / `.Application` / `.Infrastructure` の 3 レイヤーで構成される。

実メール送信、テンプレート、永続化、Outbox は本モジュールのスコープ外。
本番では `INotificationSender` の差し替えにより SMTP / 外部メール API 連携を追加する想定。

---

## 配置と依存方向

```
src/Modules/Notifications/
├── EventRegistration.Notifications.Domain          # NotificationKind, NotificationMessage
├── EventRegistration.Notifications.Application     # INotificationSender, ハンドラ
└── EventRegistration.Notifications.Infrastructure  # LoggingNotificationSender, DI 拡張
```

依存方向（Clean Architecture 準拠）:

```
Domain ← Application ← Infrastructure
                ↑
          SharedKernel.Application (IDomainEvent / IDomainEventHandler / イベント契約)
                ↑
       SharedKernel.Infrastructure (IDomainEventDispatcher 既定実装)
```

- **Notifications は Events / Registrations モジュールを直接参照しない**（CON-008 順守）。
  ドメインイベント契約は `EventRegistration.SharedKernel.Application/Events/` に置かれた
  `ParticipantConfirmedEvent` / `ParticipantPromotedFromWaitListEvent` を介してのみ結合する。
- 発行側 (Registrations) は `IDomainEventDispatcher` を呼び出すだけで Notifications を知らない。
- 購読側 (Notifications) はハンドラを DI に登録するだけで Registrations を知らない。

---

## 主要な型

### NotificationKind (Domain)

通知の種別を表す列挙型。構造化ログのキー `Kind` として出力される。

| 値 | 説明 |
|---|---|
| `ParticipantConfirmed` | 参加が新規に確定した |
| `ParticipantPromotedFromWaitList` | キャンセル待ちから参加確定に繰り上がった |

### NotificationMessage (Domain)

送信媒体に依存しない通知の最小データ。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Kind` | `NotificationKind` | 通知種別 |
| `EventId` | `Guid` | 関連イベント ID |
| `RegistrationId` | `Guid` | 関連登録 ID |
| `ParticipantName` | `string` | 参加者名 |
| `ParticipantEmail` | `string` | 参加者メールアドレス（正規化済） |

### INotificationSender (Application)

通知の送信を抽象化するポート。`Task SendAsync(NotificationMessage, CancellationToken)`。

### ハンドラ (Application)

| ハンドラ | 購読イベント |
|---|---|
| `ParticipantConfirmedNotificationHandler` | `ParticipantConfirmedEvent` |
| `ParticipantPromotedFromWaitListNotificationHandler` | `ParticipantPromotedFromWaitListEvent` |

各ハンドラはイベントを `NotificationMessage` に変換し `INotificationSender.SendAsync` を呼ぶのみの薄い実装。

### LoggingNotificationSender (Infrastructure)

`INotificationSender` の既定実装。`ILogger<LoggingNotificationSender>` を介して
構造化ログを出力する。`Console.WriteLine` 直書きは禁止。

---

## DI 登録

`Program.cs` から 1 行で配線する:

```csharp
builder.Services.AddNotificationsModule();
```

`AddNotificationsModule` の内訳:

1. `AddSharedKernelDomainEvents()` — 既定の `IDomainEventDispatcher` を登録（`TryAdd` により重複不可）。
2. `INotificationSender` → `LoggingNotificationSender` を登録。
3. 2 つのハンドラを `IDomainEventHandler<TEvent>` として登録。

Registrations 側でも `AddRegistrationsModuleInfrastructure` 内で `AddSharedKernelDomainEvents()` を
呼ぶため、Notifications モジュールが配線されていなくても Registrations のユースケースは
ディスパッチャを解決できる（ハンドラ未登録時は no-op で完了する）。

---

## ログ仕様

`LoggingNotificationSender` は `LoggerMessage` ソースジェネレータを用いて以下を出力する:

| 項目 | 値 |
|---|---|
| LogLevel | `Information` |
| EventId | `1000` |
| Message テンプレート | `Notification dispatched. Kind={Kind}, EventId={EventId}, RegistrationId={RegistrationId}, ParticipantName={ParticipantName}, ParticipantEmail={ParticipantEmail}` |

構造化フィールドとして以下を必ず含む（AC-06）:

- `Kind` (`NotificationKind`)
- `EventId` (`Guid`)
- `RegistrationId` (`Guid`)
- `ParticipantName` (`string`)
- `ParticipantEmail` (`string`)

> イベント名 (`EventName`) は Notifications から Events モジュールを参照する経路追加が必要となり
> CON-008（モジュール直接参照禁止）に違反するため、本モジュールでは含めない。
> 必要になった時点で SharedKernel のドメインイベント payload に追加するか、
> 別途反腐敗層を新設する。

---

## エラーハンドリング (AC-04)

ドメインイベントの配送は `ServiceProviderDomainEventDispatcher`（SharedKernel.Infrastructure）
が担う。各ハンドラの呼び出しは個別に `try/catch` され、例外はログに記録されるのみで
呼び出し元 (UseCase) には伝播しない。これにより:

- 通知ハンドラが未登録でもユースケースは成功する（no-op）。
- 通知ハンドラの例外が登録／キャンセルの主処理を巻き戻すことはない。

イベント発行は `SaveChangesAsync` が成功したケースのみで行われる
（`Registrations.Application.UseCases.RegisterParticipantUseCase` /
`CancelRegistrationUseCase` 参照）。

---

## 拡張方針

### 実メール送信実装への差し替え

1. `EventRegistration.Notifications.Infrastructure` に `SmtpNotificationSender` 等を実装する。
2. `NotificationsModuleInfrastructureExtensions.AddNotificationsModule` 内の登録を差し替え、
   または環境別に登録を切り替える（例: 開発環境は `LoggingNotificationSender`、
   本番は `SmtpNotificationSender`）。
3. テンプレートエンジンや件名生成ロジックは `Notifications.Application` 内に追加する
   （Domain には外部依存を持ち込まない）。

### 通知種別の追加

1. `EventRegistration.SharedKernel.Application/Events/` に新しいドメインイベント `record` を追加。
2. 発行元モジュール（例: Registrations）の UseCase で `IDomainEventDispatcher.DispatchAsync` を呼ぶ。
3. `Notifications.Application/Handlers/` に新ハンドラを追加し、`AddNotificationsModule` で登録。
4. `NotificationKind` に新しい値を追加。

### 信頼性向上 (将来の選択肢)

- Outbox パターン（DB トランザクションと同一スコープでイベント永続化）。
- 非同期ディスパッチ（`Channel<T>` / バックグラウンドサービス）。
- リトライポリシー（Polly 等）。

これらは現スコープでは導入しない。
