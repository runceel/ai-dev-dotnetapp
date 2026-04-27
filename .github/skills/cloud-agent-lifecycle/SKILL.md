---
name: cloud-agent-lifecycle
description: 'GitHub Cloud Agent の readonly GitHub CLI 制約下で feature-lifecycle を回すためのファイルベース運用スキル。Issue/PR への書き込みができない場合に、仕様・状態台帳・エージェント間ログ・PR 本文案をリポジトリ内ファイルとして管理する。'
---

# Cloud Agent Lifecycle スキル

## 概要

このスキルは、GitHub Cloud Agent で `gh` CLI または GitHub API が readonly として動作し、Issue / PR の作成・編集・コメント投稿ができない場合の代替運用を定義する。

### 主要なポイント

| 項目 | 内容 |
|------|------|
| 適用条件 | Issue / PR への書き込みができない Cloud Agent 実行環境 |
| 状態の正本 | `.github/lifecycle/<work-id>/state.yaml` |
| 仕様の正本 | `.github/lifecycle/<work-id>/spec.md` |
| Agent 間ログ | `.github/lifecycle/<work-id>/log.md` |
| GitHub 投稿 | Agent は行わず、必要な本文案を outbox に生成する |

---

## 適用条件

以下のいずれかを満たす場合、このスキルを使用する:

- GitHub Cloud Agent 上で `gh issue create/edit/comment` または `gh pr create/edit/comment` が失敗する
- `gh auth status` や環境説明から GitHub CLI が readonly と判断できる
- ユーザーが「Cloud Agent」「github cli readonly」「Issue に書き込めない」等を明示した

ローカル VS Code 環境で GitHub 書き込みが可能な場合は、既存の `feature-lifecycle` と `github-flow` を通常どおり使用してよい。

---

## Agent と transport の責務分離

Agent 定義は Product Manager / Developer / Reviewer などの役割と出力形式だけを持つ。GitHub に投稿するか、ローカルファイルへ保存するかは、このスキルの transport ルールで決める。

| 責務 | 所有者 |
|------|--------|
| 仕様・設計・実装・レビュー・テスト判断 | 各 Agent |
| 状態台帳の保存先 | `cloud-agent-lifecycle` |
| Agent 出力の保存先 | `cloud-agent-lifecycle` |
| GitHub 投稿予定本文の生成 | `cloud-agent-lifecycle` |
| 実際の GitHub 投稿・PR 作成・マージ | 人間または write 権限を持つ仕組み |

Orchestrator は、各 Agent の出力に含まれる構造化メタデータを読み取り、`state.yaml`、`log.md`、outbox を更新する。Agent 自身に GitHub Cloud Agent 固有の分岐を書かない。

---

## 基本方針

GitHub の Issue / PR を状態ストアとして使わず、リポジトリ内のファイルを正本として扱う。

| 従来の保存先 | Cloud Agent での保存先 |
|-------------|------------------------|
| SPEC Issue body | `.github/lifecycle/<work-id>/spec.md` |
| Issue / PR コメント | `.github/lifecycle/<work-id>/log.md` |
| Issue / PR body の lifecycle-state | `.github/lifecycle/<work-id>/state.yaml` |
| 実装計画コメント | `.github/lifecycle/<work-id>/implementation-plan.md` |
| PR 本文 | `.github/lifecycle/<work-id>/pull-request.md` |
| GitHub 投稿待ち本文 | `.github/lifecycle/<work-id>/outbox/*.md` |

`<work-id>` は、Issue 番号がある場合は `issue-<number>`、Issue 番号がない場合は `request-<yyyyMMddHHmm>-<slug>` とする。

---

## Cloud Agent フロー

### エントリモード

| モード | 起動条件 | Step 1.1 の挙動 |
|--------|---------|----------------|
| `cloud-new` | ユーザーが自然文で要件を指示 | Product Manager の出力を基に `spec.md` と Issue 作成用 outbox を生成 |
| `cloud-draft` | ユーザーがドラフト Issue 番号を指定（例: `draft_issue: 123`） | 既存 Issue を readonly で参照できる場合は読み取り、`spec.md` と Issue 更新用 outbox を生成 |
| `cloud-pr` | 既存 PR ブランチ上で起動 | `state.yaml` と差分から Phase 3 を復元 |

### 共通出力ルール

各 Agent の出力は、`feature-lifecycle/references/communication-protocol.md` と同じヘッダーと遷移メタデータを使用する。Cloud Agent では、その出力を GitHub コメントとして投稿せず、`log.md` に追記する。

### Phase 1: 仕様策定

1. Orchestrator は work-id を決定する。
2. Product Manager は仕様書本文を出力する。
3. Orchestrator は Product Manager の出力から `spec.md`、`state.yaml`、`log.md`、必要な outbox を作成または更新する。
4. Reviewer は `spec.md` をレビューし、Orchestrator は結果を `log.md` に追記する。
5. 仕様承認が必要な場合、Orchestrator は `state.yaml` の `status` を `WAITING_HUMAN` にする。

### Phase 2: 設計・計画

1. Architect は設計方針を出力し、Orchestrator は `design.md` に保存する。
2. Developer は実装計画を出力し、Orchestrator は `implementation-plan.md` に保存する。
3. Documentation は必要な docs 骨子を更新する。
4. Reviewer は設計・計画・docs 骨子をレビューし、結果を `log.md` に追記する。

### Phase 3: 実装

1. Developer は Cloud Agent が作業中のブランチ上で実装する。`gh pr create` は実行しない。
2. PR 本文案を `pull-request.md` に生成する。
3. Developer は PR 本文案を出力し、Orchestrator は `pull-request.md` に保存する。
4. 実装、テスト、docs 更新、レビュー、E2E 結果を `log.md` と `state.yaml` に記録する。
5. Ready for Review、マージ、Issue コメント投稿は人間または別の write 権限を持つ仕組みが行う。

---

## GitHub 書き込みの扱い

Cloud Agent モードでは、以下の操作を実行しない:

- `gh issue create`
- `gh issue edit`
- `gh issue comment`
- `gh pr create`
- `gh pr edit`
- `gh pr comment`
- `gh pr merge`

必要な投稿内容は `.github/lifecycle/<work-id>/outbox/` に Markdown として生成する。ファイル名は `NN-agent-step-target.md` の形式とする。

```text
.github/lifecycle/issue-42/outbox/
  01-product-manager-step-1.1-issue-body.md
  02-reviewer-step-1.2-issue-comment.md
  03-developer-step-2.2-plan-comment.md
  04-pr-body.md
```

---

## 状態復元

セッション開始時やコンテキスト喪失後は、次の順で状態を復元する:

1. `.github/lifecycle/**/state.yaml` を探す。
2. 対象 work-id が複数ある場合は、ユーザー指定の Issue / PR / ブランチに最も近いものを選ぶ。
3. `state.yaml` の `phase`、`step`、`status`、`last_decision` を読み、次のアクションを決める。
4. `log.md` の末尾を確認し、直近の Agent 出力と遷移メタデータを照合する。

---

## 参照リファレンス

- `references/file-transport.md` — ファイルベースの通信・outbox ルール
- `references/state-ledger.yaml.md` — Cloud Agent 用状態台帳スキーマ
- `../feature-lifecycle/references/communication-protocol.md` — Agent 出力フォーマット
- `../feature-lifecycle/references/state-transitions.md` — 状態遷移