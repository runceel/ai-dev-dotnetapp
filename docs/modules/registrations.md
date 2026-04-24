# Registrations モジュール仕様概要

> 対象: イベント参加登録システム — Registrations モジュール
> 関連: [event-registration-system-spec.md](../event-registration-system-spec.md) / [architecture.md](../architecture.md)

---

## 概要

イベントへの参加登録・キャンセル・キャンセル待ち管理を担う Bounded Context。
`EventRegistration.Registrations.Domain` / `.Application` / `.Infrastructure` の 3 レイヤーで構成される。

---

## データ構造

### Registration エンティティ

参加登録情報を表す集約ルート。

| プロパティ | 型 | 必須 | 説明 |
|---|---|---|---|
| `Id` | `Guid` | ✅ | 主キー（自動生成） |
| `EventId` | `Guid` | ✅ | 対象イベントの ID（Events モジュールの Event.Id を参照） |
| `ParticipantName` | `string` | ✅ | 参加者名 |
| `Email` | `string` | ✅ | メールアドレス |
| `Status` | `RegistrationStatus` | ✅ | 登録状態 |
| `RegisteredAt` | `DateTimeOffset` | ✅ | 登録日時 |
| `CancelledAt` | `DateTimeOffset?` | — | キャンセル日時（キャンセル時のみ） |

### RegistrationStatus 列挙型

| 値 | 説明 |
|---|---|
| `Confirmed` | 参加確定（定員以内） |
| `WaitListed` | キャンセル待ち（定員超過） |
| `Cancelled` | キャンセル済み |

### DB 構成

- `RegistrationsDbContext`（Infrastructure 層）に `DbSet<Registration>` を定義
- InMemory DB 名: `"Registrations"`（モジュール単位で一意）
- `EventId` + `Email` の組み合わせでユニーク制約（同一イベントに同じメールアドレスで重複登録を防止）

---

## 画面仕様

> Registrations モジュールの画面要素は、イベント詳細画面（`/events/{id}`）内にコンポーネントとして組み込まれる。

### 1. 参加登録フォーム（イベント詳細画面内）

| 項目 | 内容 |
|---|---|
| 表示場所 | イベント詳細画面の一部 |
| 目的 | イベントに参加登録する |

#### 入力フォーム

| フィールド | 型 | バリデーション |
|---|---|---|
| 参加者名 | テキスト | 必須 |
| メールアドレス | テキスト | 必須、メール形式 |

#### 機能

- 入力値のバリデーション
- 定員以内の場合 → ステータスを `Confirmed`（参加確定）で登録
- 定員超過の場合 → ステータスを `WaitListed`（キャンセル待ち）で登録
- 登録結果のフィードバック表示（確定 or キャンセル待ち）

---

### 2. 参加者一覧（イベント詳細画面内）

| 項目 | 内容 |
|---|---|
| 表示場所 | イベント詳細画面の一部 |
| 目的 | イベントの参加者一覧を表示する |

#### 表示内容

- 参加者名
- メールアドレス
- ステータス（確定 / キャンセル待ち）
- 登録日時

#### 機能

- 参加確定者とキャンセル待ち者を区分して表示
- 各参加者にキャンセルボタンを表示

---

### 3. キャンセル機能

| 項目 | 内容 |
|---|---|
| 表示場所 | 参加者一覧の各行 |
| 目的 | 参加登録をキャンセルする |

#### 機能

- キャンセル確認ダイアログの表示
- キャンセル実行後、ステータスを `Cancelled` に更新
- **キャンセル待ち繰り上げ**: 確定者がキャンセルした場合、キャンセル待ちの先頭（`RegisteredAt` が最も古い `WaitListed`）を自動的に `Confirmed` に繰り上げる
- 一覧の即時更新

---

## ビジネスルール

| ルール | 説明 |
|---|---|
| 定員チェック | 現在の `Confirmed` 件数が定員未満なら `Confirmed`、以上なら `WaitListed` |
| 重複登録防止 | 同一イベント × 同一メールアドレスの組み合わせで有効な登録（`Confirmed` or `WaitListed`）が既にある場合は登録不可 |
| キャンセル待ち繰り上げ | `Confirmed` の参加者がキャンセルした場合、`WaitListed` の中で `RegisteredAt` が最も古い登録を `Confirmed` に自動変更 |
| キャンセル後の再登録 | キャンセル済みの参加者は同一メールアドレスで再登録可能 |

---

## モジュール間連携

| 連携先 | 方向 | 内容 |
|---|---|---|
| Events | Registrations → Events | 参加登録時に定員（`Capacity`）を取得して定員チェックを行う |
| Events | Events → Registrations | イベント詳細画面で対象イベントの参加者一覧・登録フォームを表示するためにデータを取得 |
