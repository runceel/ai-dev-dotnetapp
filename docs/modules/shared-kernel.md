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
