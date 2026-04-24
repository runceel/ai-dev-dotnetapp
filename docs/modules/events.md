# Events モジュール仕様概要

> 対象: イベント参加登録システム — Events モジュール
> 関連: [event-registration-system-spec.md](../event-registration-system-spec.md) / [architecture.md](../architecture.md)

---

## 概要

イベント（開催情報）の作成・一覧表示・詳細表示を担う Bounded Context。
`EventRegistration.Events.Domain` / `.Application` / `.Infrastructure` の 3 レイヤーで構成される。

---

## データ構造

### Event エンティティ

イベント情報を表す集約ルート。

| プロパティ | 型 | 必須 | 説明 |
|---|---|---|---|
| `Id` | `Guid` | ✅ | 主キー（自動生成） |
| `Name` | `string` | ✅ | イベント名 |
| `Description` | `string` | — | イベントの説明 |
| `ScheduledAt` | `DateTimeOffset` | ✅ | 開催日時 |
| `Capacity` | `int` | ✅ | 定員（1 以上） |
| `CreatedAt` | `DateTimeOffset` | ✅ | 作成日時 |

### DB 構成

- `EventsDbContext`（Infrastructure 層）に `DbSet<Event>` を定義
- InMemory DB 名: `"Events"`（モジュール単位で一意）

---

## 画面仕様

### 1. イベント一覧画面

| 項目 | 内容 |
|---|---|
| パス | `/events` |
| 目的 | 登録済みイベントの一覧をカード形式で表示する |

#### 表示内容

- イベント名
- 開催日時
- 定員
- 説明（概要のみ）

#### 機能

- イベントをカード形式で一覧表示（開催日時の降順）
- カードクリックでイベント詳細画面へ遷移
- イベント作成画面へのリンク / ボタン

---

### 2. イベント作成画面

| 項目 | 内容 |
|---|---|
| パス | `/events/create` |
| 目的 | 新しいイベントを作成する |

#### 入力フォーム

| フィールド | 型 | バリデーション |
|---|---|---|
| イベント名 | テキスト | 必須 |
| 開催日時 | 日時ピッカー | 必須 |
| 定員 | 数値 | 必須、1 以上 |
| 説明 | テキストエリア | 任意 |

#### 機能

- 入力値のバリデーション
- 保存成功時、イベント詳細画面へ遷移
- キャンセルボタンで一覧画面へ戻る

---

### 3. イベント詳細画面

| 項目 | 内容 |
|---|---|
| パス | `/events/{id}` |
| 目的 | イベントの詳細情報を表示する |

#### 表示内容

- イベント名
- 開催日時
- 定員
- 説明
- 残り枠数（定員 − 確定参加者数）

#### 機能

- イベント情報の閲覧
- **Registrations モジュールの参加登録フォーム・参加者一覧をこの画面内に表示**（モジュール連携）
- 一覧画面へ戻るリンク

---

## モジュール間連携

| 連携先 | 方向 | 内容 |
|---|---|---|
| Registrations | Events → Registrations | イベント詳細画面で参加登録フォーム・参加者一覧を表示するために Registrations モジュールの機能を利用 |
| Registrations | Registrations → Events | 参加登録時に定員を確認するため Events モジュールからイベント情報（定員）を取得 |
