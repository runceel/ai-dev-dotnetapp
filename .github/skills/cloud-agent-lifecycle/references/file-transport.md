# ファイルトランスポート

## 概要

このドキュメントは、GitHub Cloud Agent が Issue / PR へ書き込めない場合に、Agent 間の通信と GitHub 投稿予定内容をリポジトリ内ファイルで管理するためのルールを定義する。

### 主要なポイント

| 項目 | 内容 |
|------|------|
| 通信ログ | `.github/lifecycle/<work-id>/log.md` に追記する |
| 投稿予定 | `.github/lifecycle/<work-id>/outbox/*.md` に保存する |
| 状態更新 | `.github/lifecycle/<work-id>/state.yaml` を更新する |
| 既存 GitHub コメント | readonly で取得できる場合のみ入力情報として読む |

---

## ディレクトリ構成

```text
.github/lifecycle/<work-id>/
  spec.md
  design.md
  implementation-plan.md
  pull-request.md
  e2e-report.md
  log.md
  state.yaml
  outbox/
    NN-agent-step-target.md
```

`design.md`、`implementation-plan.md`、`pull-request.md`、`e2e-report.md` は、該当フェーズに到達した時点で作成する。

---

## log.md の形式

`log.md` は append-only とし、既存エントリの意味を変える編集を行わない。誤りを訂正する場合は、新しいエントリとして訂正内容を追記する。

````markdown
## 2026-04-27T10:00:00Z — Product Manager — Step 1.1

> **[Product Manager]** — Step 1.1: 仕様書作成
> Phase 1 / レビューサイクル 1回目

本文...

```yaml
# --- transition metadata ---
agent: "Product Manager"
phase: 1
step: "1.1"
review_cycle: 1
decision: "DONE"
next_step: "1.2"
artifacts:
  - ".github/lifecycle/issue-42/spec.md"
```
````

---

## outbox の形式

GitHub に投稿したい内容は、投稿先ごとに outbox ファイルとして保存する。

| 投稿予定先 | ファイル名例 |
|-----------|--------------|
| Issue body | `01-product-manager-step-1.1-issue-body.md` |
| Issue comment | `02-reviewer-step-1.2-issue-comment.md` |
| PR body | `04-pr-body.md` |
| PR comment | `05-tester-step-3.7-pr-comment.md` |
| E2E result | `05-tester-step-3.7-e2e-result.md` |

outbox ファイルの先頭には、投稿先メタデータを HTML コメントで記録する。

```markdown
<!-- agent-outbox
target: "issue"
target_number: 42
operation: "comment"
created_by: "Reviewer"
created_at: "2026-04-27T10:15:00Z"
-->

> **[Reviewer]** — Step 1.2: 仕様レビュー
> Phase 1 / レビューサイクル 1回目

本文...
```

---

## GitHub 投稿との関係

Cloud Agent は outbox を作成するだけで、GitHub への投稿は行わない。投稿が必要な場合は、人間または write 権限を持つ GitHub Actions が outbox の内容を確認して実行する。

投稿後も outbox ファイルは履歴として残してよい。投稿済み管理が必要な場合は、別コミットでファイル名に `.posted` を付けるか、`state.yaml` の `outbox_status` に記録する。