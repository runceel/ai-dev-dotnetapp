---
name: feature-lifecycle
description: '7 つのカスタムエージェント（Orchestrator / Product Manager / Architect / Developer / Reviewer / Tester / Documentation）が協調して機能開発を自律的に進めるワークフロースキル。仕様策定から実装・テスト・マージ、およびドキュメント追従までの全ライフサイクルを定義する。'
---

# Feature Lifecycle スキル

Orchestrator エージェントが駆動する、機能開発の自律ワークフローを定義する。

---

## エージェント一覧

| エージェント | 役割 | ツール権限 |
|---|---|---|
| **Orchestrator** | ライフサイクル駆動・状態管理・委譲 | 全ツール + agent |
| **Product Manager** | 仕様書 Issue 作成・要件管理 | read / search / execute |
| **Architect** | 設計レビュー・完成判断 | read / search / execute |
| **Developer** | 実装・ユニットテスト・指摘反映 | 全ツール + agent |
| **Reviewer** | 仕様/コードレビュー | read / search / execute |
| **Tester** | E2E テスト実行・バグ報告 | read / search / execute |
| **Documentation** | 設計ドキュメント骨子生成・実装追従 docs 更新 | read / edit / search / execute |

---

## エントリモード

Phase 1 の開始時に、ユーザーのリクエスト形態に応じて 2 つのモードから選択する:

| モード | 起動条件 | Step 1.1 の挙動 |
|--------|---------|----------------|
| **`new`**（デフォルト） | ユーザーが自然文で要件を指示 | Product Manager が `create-specification` で**新規 SPEC Issue** を作成 |
| **`draft`** | ユーザーがドラフト Issue 番号を指定（例: `draft_issue: 123`） | Product Manager が `update-specification` で**指定 Issue を SPEC 化**（新規 Issue は作らない）。以降その Issue を SPEC Issue として扱う |

両モードとも Step 1.2 以降（仕様レビュー・設計・実装・テスト）は共通フローに合流する。状態台帳の `entry_mode` フィールドに選択したモードを記録する（`references/state-ledger.md` 参照）。

---

## Phase 1: 仕様策定（🧑‍💼 人間チェックポイントあり）

| Step | 担当 | 内容 |
|------|------|------|
| 1.1 | Product Manager | エントリモードに応じて仕様書 Issue を作成（`new`）または既存 Issue を SPEC 化（`draft`） |
| 1.2 | Reviewer | 仕様書をレビュー → APPROVE / REQUEST CHANGES |
| 1.3 | Product Manager | REQUEST CHANGES の場合、指摘を反映して 1.2 へ戻る |
| 1.4 | — | APPROVE の場合、仕様確定 |
| 1.5 | 🧑‍💼 **人間** | **仕様の最終承認**（ここで人間が判断を挟む） |

- 仕様レビューループ上限: **3 回**（超過時は人間にエスカレーション）
- `draft` モードでも仕様レビュー（Step 1.2）は**必ず実施**する

---

## Phase 2: 設計・計画（🧑‍💼 設計レビューあり）

| Step | 担当 | 内容 |
|------|------|------|
| 2.1 | Architect | 仕様書を基に設計方針を策定 |
| 2.2 | Developer | 実装計画を仕様書 Issue にコメントとして投稿（`communication-protocol.md` のフォーマットに従う） |
| 2.2.5 | Documentation | 設計方針・実装計画を基に `docs/` 配下の設計ドキュメント骨子を作成/更新 |
| 2.3 | Reviewer | 設計・計画・ドキュメント骨子をレビュー → APPROVE / REQUEST CHANGES |
| 2.4 | Architect / Developer / Documentation | REQUEST CHANGES の場合、指摘を反映して 2.3 へ戻る |

- 設計レビューループ上限: **2 回**（超過時は人間にエスカレーション）

### 設計・計画の必須成果物

Phase 3 に進むためには、以下が設計・計画に含まれていること:

| 成果物 | 説明 |
|--------|------|
| 影響モジュール一覧 | 変更が及ぶモジュールとレイヤーを列挙 |
| API 変更 | 新規・変更されるエンドポイント、DTO の定義 |
| テスト戦略 | ユニットテスト・E2E テストの対象と方針 |
| リスク・制約 | 既知のリスク、パフォーマンス影響、移行計画 |

---

## Phase 3: 実装（🤖 自律ループ）

| Step | 担当 | 内容 |
|------|------|------|
| 3.1 | Developer | feature ブランチ作成・Draft PR 作成（初期コミット付き） |
| 3.2 | Developer | 実装・ユニットテスト |
| 3.3 | Developer | ビルド確認（`cd src && dotnet build {ProjectName}.slnx`） |
| 3.4 | Orchestrator | 実装完了を検知 |
| 3.4.5 | Documentation | 実装差分を解析し、影響を受ける `docs/` / `README.md` / `docs/tests/` を更新し同 PR にコミット |
| 3.4.9 | Orchestrator | PR を Ready for Review に変更 |
| 3.5 | Reviewer | コードレビュー（コード + docs 更新）→ APPROVE / REQUEST CHANGES |
| 3.6 | Developer / Documentation | REQUEST CHANGES の場合、指摘を反映して 3.5 へ戻る |
| 3.7 | Tester | E2E テスト実行 → PASS / FAIL |
| 3.8 | Developer | FAIL の場合、バグ修正して 3.7 へ戻る |
| 3.9 | Architect | 完成判断（docs 最新化も判定基準に含む）→ COMPLETE / INCOMPLETE |
| 3.10 | Developer / Documentation | INCOMPLETE の場合、改善して 3.5 へ戻る |
| 3.11 | 🧑‍💼 **人間** | **マージの最終承認**（ここで人間が判断を挟む） |
| 3.12 | Orchestrator | 承認後 → Squash Merge、ブランチ削除 |

- コードレビューループ上限: **3 回**
- ビルド失敗ループ上限: **5 回**
- 完成判断サイクル上限: **3 回**
- ドキュメント更新ループ上限: **3 回**
- いずれも超過時は人間にエスカレーション

---

## 遷移要約

1. エントリモード判定: `draft_issue` 指定があれば `draft`、なければ `new`
2. Phase 1 → Phase 2: 人間が仕様を承認
3. Phase 2 → Phase 3: 設計方針・実装計画・ドキュメント骨子が設計レビュー承認後、設計・計画が完了
4. Phase 3 内: Draft PR 作成 → 実装 → docs 更新 → Ready for Review → Reviewer APPROVE → Tester PASS → Architect COMPLETE（docs 最新化含む）→ 人間が承認 → マージ
5. 各ループで REQUEST CHANGES / FAIL / INCOMPLETE → 修正して再挑戦
6. ループ上限超過 → 人間にエスカレーション

---

## リファレンス

詳細な定義は `references/` に分離し、必要時のみロードする:

| ファイル | 内容 | ロードタイミング |
|---------|------|-----------------|
| `references/communication-protocol.md` | コメント投稿ルール・フォーマット例 | コメント投稿時 |
| `references/state-transitions.md` | 状態遷移の詳細定義 | 遷移判断時 |
| `references/state-ledger.md` | 状態台帳スキーマ・操作手順 | 状態管理・復元時 |
