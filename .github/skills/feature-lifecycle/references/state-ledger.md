# 状態台帳（State Ledger）

Orchestrator が管理する、機能開発の現在状態を永続化するためのスキーマ定義。

---

## 概要

feature-lifecycle の各機能開発には **1 つの状態台帳** が紐づく。台帳は対象の **Issue body**（Phase 1-2）または **PR body**（Phase 3）に YAML メタデータブロックとして埋め込まれ、Orchestrator が遷移のたびに更新する。

---

## YAML メタデータブロック

Issue / PR の本文末尾に、以下の形式で状態台帳を記載する:

```yaml
<!-- lifecycle-state
feature: "<機能名の短い説明>"
entry_mode: "<new|draft>"
issue: <仕様書 Issue 番号>
pr: <PR 番号（Phase 3 で設定）>
branch: "<feature ブランチ名（Phase 3 で設定）>"
plan_comment_url: "<承認済み実装計画コメントの permalink（Step 2.3 APPROVE 後に設定）>"
phase: <現在のフェーズ番号>
step: "<現在のステップ番号>"
status: "<ACTIVE|WAITING_HUMAN|BLOCKED|ENV_FAILED|COMPLETED>"
last_agent: "<最後に作業したエージェント名>"
last_decision: "<最後の遷移キーワード>"
counters:
  spec_review: <仕様レビュー回数>
  design_review: <設計レビュー回数>
  build_fail: <ビルド失敗回数>
  code_review: <コードレビュー回数>
  completion_cycle: <完成判断サイクル回数>
  e2e_retry: <E2E テスト再実行回数>
  doc_update: <ドキュメント更新ループ回数>
updated_at: "<ISO 8601 タイムスタンプ>"
-->
```

> **Note:** HTML コメント `<!-- -->` で囲むことで、GitHub 上の Issue / PR 表示ではメタデータが非表示になり、本文の可読性を保つ。

---

## フィールド定義

| フィールド | 必須 | 型 | 説明 |
|-----------|------|-----|------|
| `feature` | ✅ | string | 機能の短い説明（ブランチ名の `-` 以降に対応） |
| `entry_mode` | ✅ | string | `new`（新規 SPEC Issue 作成） / `draft`（既存ドラフト Issue を SPEC 化） |
| `issue` | ✅ | number | 仕様書 Issue の番号（`draft` モードではユーザー指定のドラフト Issue 番号） |
| `pr` | — | number | PR 番号。Phase 3 の Step 3.1 で設定 |
| `branch` | — | string | feature ブランチ名。Phase 3 の Step 3.1 で設定 |
| `plan_comment_url` | — | string | 承認済み実装計画コメントの permalink。Step 2.3 APPROVE 後に設定 |
| `phase` | ✅ | number | 現在のフェーズ（1, 2, 3） |
| `step` | ✅ | string | 現在のステップ（例: "1.2", "3.5"） |
| `status` | ✅ | string | `ACTIVE` / `WAITING_HUMAN` / `BLOCKED` / `ENV_FAILED` / `COMPLETED` |
| `last_agent` | ✅ | string | 最後に作業を完了したエージェント名 |
| `last_decision` | — | string | 最後の遷移キーワード（APPROVE, FAIL 等） |
| `counters` | ✅ | object | 各ループのリトライカウンター |
| `updated_at` | ✅ | string | 最終更新の ISO 8601 タイムスタンプ |

### counters フィールド

| カウンター | 上限 | 対象ループ |
|-----------|------|-----------|
| `spec_review` | 3 | Phase 1 仕様レビュー |
| `design_review` | 2 | Phase 2 設計レビュー |
| `build_fail` | 5 | Phase 3 ビルド失敗 |
| `code_review` | 3 | Phase 3 コードレビュー |
| `completion_cycle` | 3 | Phase 3 完成判断 |
| `e2e_retry` | 2 | Phase 3 E2E テスト再実行 |
| `doc_update` | 3 | Phase 3 ドキュメント更新ループ |

---

## Orchestrator の操作手順

### 1. 台帳の作成（Phase 1 開始時）

Product Manager が仕様書 Issue を作成（または既存ドラフト Issue を SPEC 化）した直後に、Issue body の末尾に初期状態の台帳を追記する。`entry_mode` には `new`（新規作成）または `draft`（既存ドラフト Issue の SPEC 化）を設定する:

```bash
# Issue body の末尾にメタデータを追記
gh issue edit <number> --body "$(gh issue view <number> --json body -q .body)

<!-- lifecycle-state
feature: \"<機能名>\"
entry_mode: \"<new|draft>\"
issue: <number>
pr:
branch:
plan_comment_url:
phase: 1
step: \"1.2\"
status: \"ACTIVE\"
last_agent: \"Product Manager\"
last_decision: \"DONE\"
counters:
  spec_review: 0
  design_review: 0
  build_fail: 0
  code_review: 0
  completion_cycle: 0
  e2e_retry: 0
  doc_update: 0
updated_at: \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"
-->"
```

### 2. 台帳の更新（各遷移時）

遷移のたびに、台帳の該当フィールドを更新する。更新対象:

- `phase` / `step`: 新しいフェーズ・ステップに更新
- `status`: 必要に応じて更新（WAITING_HUMAN, BLOCKED 等）
- `last_agent` / `last_decision`: 直前のエージェント出力から取得
- `plan_comment_url`: Step 2.3 で設計・計画が APPROVE された際に、承認対象の実装計画コメントの permalink を設定
- `counters`: ループ発生時にインクリメント
- `updated_at`: 現在のタイムスタンプ

### 3. Phase 3 移行時の台帳移動

Phase 2 → Phase 3 への遷移時:

1. Developer が Draft PR を作成（Step 3.1）
2. Orchestrator が Issue body から台帳を読み取り
3. PR body の末尾に台帳をコピー（`pr` と `branch` フィールドを設定）
4. 以降は PR body の台帳を更新対象とする

### 4. 台帳の読み取り（状態復元）

セッション開始時やコンテキスト喪失後:

1. 対象の Issue / PR body から `<!-- lifecycle-state ... -->` ブロックを抽出
2. YAML をパースして現在の状態を復元
3. `step` フィールドから次のアクションを決定

---

## エスカレーション判定

台帳の `counters` が上限に達した場合、Orchestrator は自動的に人間にエスカレーションする:

```
if counters.spec_review > 3 → エスカレーション
if counters.design_review > 2 → エスカレーション
if counters.build_fail > 5 → エスカレーション
if counters.code_review > 3 → エスカレーション
if counters.completion_cycle > 3 → エスカレーション
if counters.e2e_retry > 2 → FLAKY_TEST として報告
if counters.doc_update > 3 → エスカレーション
```

---

## 使用例

### Phase 1 進行中の Issue body 末尾

```yaml
<!-- lifecycle-state
feature: "AI チャット履歴のエクスポート機能"
entry_mode: "new"
issue: 42
pr:
branch:
plan_comment_url:
phase: 1
step: "1.3"
status: "ACTIVE"
last_agent: "Reviewer"
last_decision: "REQUEST_CHANGES"
counters:
  spec_review: 1
  design_review: 0
  build_fail: 0
  code_review: 0
  completion_cycle: 0
  e2e_retry: 0
  doc_update: 0
updated_at: "2026-04-15T10:30:00Z"
-->
```

### Phase 3 進行中の PR body 末尾

```yaml
<!-- lifecycle-state
feature: "AI チャット履歴のエクスポート機能"
entry_mode: "draft"
issue: 42
pr: 45
branch: "feature/42-export-chat-history"
plan_comment_url: "https://github.com/owner/repo/issues/42#issuecomment-1234567890"
phase: 3
step: "3.7"
status: "ACTIVE"
last_agent: "Reviewer"
last_decision: "APPROVE"
counters:
  spec_review: 2
  design_review: 1
  build_fail: 0
  code_review: 1
  completion_cycle: 0
  e2e_retry: 0
  doc_update: 1
updated_at: "2026-04-15T14:00:00Z"
-->
```
