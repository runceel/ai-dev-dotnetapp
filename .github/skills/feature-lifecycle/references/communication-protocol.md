# コミュニケーションプロトコル

エージェント間のやりとりは、対象の Issue / PR にコメントとして記録する。

---

## 投稿先ルール

| フェーズ | 投稿先 | コマンド |
|---------|--------|---------|
| Phase 1-2（PR 未作成） | Issue | `gh issue comment <number> --body "..."` |
| Phase 3（Step 3.1 で Draft PR 作成済み） | PR | `gh pr comment <number> --body "..."` |

> **Note:** Phase 3 では Step 3.1 で Draft PR が作成されるため、全ステップで PR にコメント可能です。

---

## コメントヘッダーフォーマット

すべてのエージェントコメントは、以下の共通ヘッダーで始める:

```
> **[エージェント名]** — Step N.N: ステップ名
> Phase N / レビューサイクル N回目
```

---

## 投稿タイミング

- 各ステップの **開始時** と **完了時** にコメントを投稿する
- 遷移判断キーワード（APPROVE, REQUEST CHANGES, COMPLETE, INCOMPLETE, PASS, FAIL）は **必ずコメントに含める**

---

## 構造化メタデータ（遷移フッター）

すべての **完了コメント** の末尾に、以下の YAML コードブロックを付与する。Orchestrator はこのブロックを解析して遷移判断を行う。

### フォーマット

```yaml
# --- transition metadata ---
agent: "<エージェント名>"
phase: <フェーズ番号>
step: "<ステップ番号>"
review_cycle: <レビューサイクル番号>
decision: "<判定キーワード>"
next_step: "<推奨される次ステップ番号>"
artifacts:
  - "<成果物の説明>"
```

### フィールド定義

| フィールド | 必須 | 説明 |
|-----------|------|------|
| `agent` | ✅ | コメントを投稿したエージェント名 |
| `phase` | ✅ | 現在のフェーズ番号（1, 2, 3） |
| `step` | ✅ | 現在のステップ番号（例: "3.5"） |
| `review_cycle` | ✅ | レビューサイクルの回数（初回は 1） |
| `decision` | ✅ | `APPROVE` / `REQUEST_CHANGES` / `COMPLETE` / `INCOMPLETE` / `PASS` / `FAIL` / `DONE` |
| `next_step` | — | 推奨される次ステップ。Orchestrator が最終判断する |
| `artifacts` | — | このステップで作成・変更した成果物のリスト |

### 使用例

```yaml
# --- transition metadata ---
agent: "Reviewer"
phase: 3
step: "3.5"
review_cycle: 1
decision: "APPROVE"
next_step: "3.7"
artifacts: []
```

### 注意事項

- **開始コメント** にはメタデータブロック不要（完了コメントのみ）
- Orchestrator は `decision` フィールドで遷移を判断する（本文中のキーワードは補助）
- `decision` の値は必ず上記の定義済みキーワードのいずれかとする
- 遷移判断がないステップ（Developer の実装完了等）では `DONE` を使用する

---

## 実装計画コメントフォーマット（Step 2.2）

Developer が Phase 2 Step 2.2 で仕様書 Issue に投稿する実装計画コメントは、以下の構造に従う。

### テンプレート

```markdown
> **[Developer]** — Step 2.2: 実装計画
> Phase 2 / レビューサイクル N回目

## 実装計画

### スコープ

- **対象**: この機能で実装する範囲
- **対象外**: 明示的に含めないもの

### 影響モジュール・レイヤー

| モジュール | レイヤー | 変更内容 |
|-----------|---------|---------|
| Conversation | Domain | 新規エンティティ追加 |
| Conversation | Application | UseCase 追加 |
| Conversation | Infrastructure | リポジトリ実装 |

### API / DTO / コントラクト変更

- 新規・変更されるエンドポイント、DTO の定義

### アーキテクチャ制約・方針

- Architect が Step 2.1 で決定した制約・非機能要件
- Clean Architecture のレイヤー依存方向の遵守事項

### テスト戦略

- ユニットテスト: 対象と方針
- E2E テスト: 対象と方針

### リスク・制約・移行に関する注意事項

- 既知のリスク、パフォーマンス影響、後方互換性、移行計画
```

### 注意事項

- Orchestrator は **承認された計画コメントの permalink** を状態台帳に記録する（`references/state-ledger.md` 参照）
- Reviewer は Step 2.3 でこのコメントをレビュー対象とする
- REQUEST CHANGES の場合、Developer は **新しいコメント** として修正版を投稿する（既存コメントの編集ではなく、履歴を残す）

---

## エージェント別コメント例

### Product Manager（Step 1.1: 仕様書作成）

```markdown
> **[Product Manager]** — Step 1.1: 仕様書作成
> Phase 1 / 初回

## 仕様書を作成しました

Issue #XX として仕様書を作成しました。
受け入れ基準: N 項目
レビューをお願いします。
```

### Reviewer（Step 1.2: 仕様レビュー）

```markdown
> **[Reviewer]** — Step 1.2: 仕様レビュー
> Phase 1 / レビューサイクル 1回目

## レビュー結果

- **BLOCKER**: 受け入れ基準 #3 が曖昧です
- **SUGGESTION**: ユーザーストーリーにペルソナを追加してください

**REQUEST CHANGES**
```

### Developer（Step 3.2: 実装）

```markdown
> **[Developer]** — Step 3.2: 実装
> Phase 3 / レビューサイクル 1回目

## 実装完了

- ブランチ: `feature/XX-feature-name`
- 変更ファイル: N ファイル
- ユニットテスト: N テスト追加、全パス
- ビルド: 成功
```

### Tester（Step 3.7: E2E テスト）

```markdown
> **[Tester]** — Step 3.7: E2E テスト
> Phase 3 / レビューサイクル 1回目

## テスト結果

| シナリオ | 結果 |
|---------|------|
| ログインしてチャット送信 | ✅ PASS |
| データソース追加 | ❌ FAIL |

**FAIL** — 1 件のシナリオが失敗しました。
```

### Architect（Step 3.9: 完成判断）

```markdown
> **[Architect]** — Step 3.9: 完成判断
> Phase 3 / レビューサイクル 1回目

## 完成判断

- 仕様充足: ✅
- アーキテクチャ整合性: ✅
- テスト網羅: ✅

**COMPLETE** — マージ可能です。
```
