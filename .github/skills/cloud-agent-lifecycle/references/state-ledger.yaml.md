# Cloud Agent 状態台帳

## 概要

このドキュメントは、GitHub Cloud Agent モードで使用する状態台帳 `state.yaml` のスキーマを定義する。Issue / PR body に状態を埋め込まず、リポジトリ内ファイルを正本として扱う。

### 主要なポイント

| 項目 | 内容 |
|------|------|
| 保存先 | `.github/lifecycle/<work-id>/state.yaml` |
| 更新者 | Orchestrator |
| 復元キー | `work_id`, `issue`, `branch`, `pr` |
| 遷移判断 | `phase`, `step`, `status`, `last_decision` |

---

## スキーマ

```yaml
work_id: "issue-42"
feature: "イベント一覧の検索機能"
entry_mode: "cloud-draft"
transport: "repo-files"
issue: 42
pr:
branch: "feature/42-event-search"
artifacts:
  spec: ".github/lifecycle/issue-42/spec.md"
  design: ".github/lifecycle/issue-42/design.md"
  implementation_plan: ".github/lifecycle/issue-42/implementation-plan.md"
  pull_request: ".github/lifecycle/issue-42/pull-request.md"
  e2e_report: ".github/lifecycle/issue-42/e2e-report.md"
  log: ".github/lifecycle/issue-42/log.md"
phase: 1
step: "1.2"
status: "ACTIVE"
last_agent: "Product Manager"
last_decision: "DONE"
counters:
  spec_review: 0
  design_review: 0
  build_fail: 0
  code_review: 0
  completion_cycle: 0
  e2e_retry: 0
  doc_update: 0
outbox_status:
  pending: 1
  posted: 0
updated_at: "2026-04-27T10:30:00Z"
```

---

## フィールド定義

| フィールド | 必須 | 説明 |
|-----------|------|------|
| `work_id` | はい | ライフサイクル作業単位の ID |
| `feature` | はい | 機能の短い説明 |
| `entry_mode` | はい | `cloud-new` / `cloud-draft` / `cloud-pr` |
| `transport` | はい | Cloud Agent では `repo-files` 固定 |
| `issue` | いいえ | 関連 Issue 番号。未作成の場合は空 |
| `pr` | いいえ | 関連 PR 番号。Cloud Agent が取得できない場合は空 |
| `branch` | いいえ | 作業ブランチ名 |
| `artifacts` | はい | 仕様、設計、計画、E2E レポート、ログなどのファイルパス |
| `phase` | はい | 現在フェーズ |
| `step` | はい | 現在ステップ |
| `status` | はい | `ACTIVE` / `WAITING_HUMAN` / `BLOCKED` / `ENV_FAILED` / `COMPLETED` |
| `last_agent` | はい | 最後に作業した Agent |
| `last_decision` | いいえ | 最後の遷移キーワード |
| `counters` | はい | 各レビュー・再試行カウンター |
| `outbox_status` | いいえ | GitHub 投稿待ちファイルの状態 |
| `updated_at` | はい | ISO 8601 形式の更新日時 |

---

## 更新ルール

- Orchestrator は各ステップ完了後に `state.yaml` を更新する。
- `log.md` に完了エントリを追記してから `state.yaml` を更新する。
- `status: WAITING_HUMAN` の間は、自律的に次フェーズへ進まない。
- GitHub 書き込み失敗は `AUTH_FAILED` として扱わず、Cloud Agent モードでは通常制約として `transport: repo-files` を維持する。