---
name: quick-fix
description: 'feature-lifecycle で作成済みの PR に対して、レビュー指摘やユーザーの直接指示による微修正を軽量フローで反映するスキル。Orchestrator から Developer に委譲して実行する。'
---

# Quick Fix スキル

feature-lifecycle で作成された PR に対する **微修正ワークフロー**。
仕様策定・設計・E2E テスト等の重いフェーズをスキップし、Developer エージェントが直接修正を行う。

---

## 適用条件

以下の **すべて** を満たす場合に使用する:

- 既存の PR（feature-lifecycle で作成済み）が存在する
- 修正内容が小規模である（UI 調整、テキスト変更、バグ修正、レビュー指摘対応 等）
- 新規機能の追加ではない

> **判断基準**: 修正がアーキテクチャや仕様に影響しない範囲であること。影響がある場合は `feature-lifecycle` に切り替える。

### ハード条件（1 つでも違反したら feature-lifecycle に切り替え）

以下のいずれかに該当する場合は quick-fix を使用**してはならない**。自動的に `feature-lifecycle` へフォールバックする:

| 条件 | 理由 |
|------|------|
| 公開 API（エンドポイント・リクエスト/レスポンス型）の変更 | 互換性影響の評価が必要 |
| Domain 層（エンティティ・値オブジェクト・ドメインサービス）の変更 | 設計レビューが必要 |
| 複数モジュールにまたがる変更 | 結合度への影響評価が必要 |
| データベーススキーマ（Cosmos DB コンテナ定義等）の変更 | マイグレーション計画が必要 |
| 変更ファイル数が **5 ファイル** を超える | 影響範囲が大きく full review が必要 |
| 新しい NuGet パッケージの追加 | 依存関係の評価が必要 |

> **自動判定**: Orchestrator または Developer は、修正着手前にこのハード条件を確認する。条件違反が検出された場合は、ユーザーに `feature-lifecycle` への切り替えを提案し、承認を得てから進める。

---

## 入力

| 入力 | 必須 | 説明 |
|------|------|------|
| PR 番号 | ✅ | 対象の Pull Request を特定する |
| 修正指示 | ✅ | 下記いずれかの方法で取得する |

### 修正指示の取得方法

- **A) PR レビューコメントから自動読取**: `gh pr view <番号>` および review comments を取得し、未解決の指摘を修正対象とする
- **B) ユーザーからの直接指示**: 「ここを直して」等のユーザー発言をそのまま修正指示とする
- **C) A + B の組み合わせ**: レビューコメントに加え、追加の修正指示がある場合

---

## フロー

### Orchestrator 経由の場合

```
Orchestrator: 修正リクエスト受領
  → Developer に委譲（quick-fix モード）
    → Developer: 修正実施（下記手順）
  → Reviewer: コードレビュー（APPROVE / REQUEST CHANGES）
    → REQUEST CHANGES の場合: Developer が修正 → 再レビュー（上限 3 回）
  → Orchestrator: 完了報告
```

Orchestrator は Developer / Reviewer への委譲と完了報告を行う。
**Tester / Architect の介入は行わない。**

### Developer が直接実行する場合

Developer が下記の修正手順をそのまま実行し、完了後に Reviewer がレビューを行う。

---

## 修正手順（Developer）

| Step | 内容 |
|------|------|
| 1 | 対象 PR の feature ブランチにいることを確認する（別ブランチにいる場合のみ `git switch <branch>`） |
| 2 | 修正内容を把握する |
| 2a | レビューコメント読取の場合: `gh pr view <番号>` + review comments を確認 |
| 2b | ユーザー指示の場合: 指示内容を確認 |
| 3 | コード修正を実施 |
| 4 | ビルド & テスト確認 |
| 5 | Conventional Commits でコミット |
| 6 | 既存 PR にプッシュ（`git push`） |
| 7 | レビューコメントへの返信（該当する場合） |

### Step 4: ビルド & テスト

```bash
dotnet build {ProjectName}.slnx && dotnet test --solution {ProjectName}.slnx
```

ビルドまたはテストが失敗した場合は修正して再実行する（最大 **3 回**）。
3 回失敗した場合はユーザーにエスカレーションする。

### Step 5: コミット

```bash
git add .
git commit -m "fix(<scope>): <修正内容の説明>"
```

- type は基本 `fix` を使用する
- 修正内容に応じて `style`, `refactor`, `docs` 等も可
- scope は `github-flow` スキルの scope 規約に従う

### Step 7: レビューコメントへの返信

レビューコメントから読み取った指摘に対しては、修正完了後に PR コメントで返信する:

```bash
gh pr comment <番号> --body "> **[Developer]** — Quick Fix 完了
>
> 以下の指摘を修正しました:
> - <指摘1の要約>: <対応内容>
> - <指摘2の要約>: <対応内容>
>
> コミット: <SHA>"
```

---

## コードレビュー（Reviewer）

Developer の修正完了後、Reviewer がコードレビューを行う。

| Step | 内容 |
|------|------|
| R1 | Reviewer が PR の差分をレビュー → APPROVE / REQUEST CHANGES |
| R2 | REQUEST CHANGES の場合、Developer が指摘を修正してプッシュ → R1 へ戻る |

- レビューループ上限: **3 回**（超過時はユーザーにエスカレーション）
- Reviewer は `communication-protocol.md` に従い、PR にコメントとして結果を投稿する
- APPROVE 後、Orchestrator が完了報告を行う

---

## 必須事項

| 項目 | ルール |
|------|--------|
| ビルド成功 | `dotnet build {ProjectName}.slnx` が通ること |
| テスト成功 | `dotnet test --solution {ProjectName}.slnx` が通ること |
| Conventional Commits | `fix(<scope>): ...` 形式でコミット |
| 既存ブランチ | 新しいブランチは作成しない |
| 既存 PR | 新しい PR は作成しない |

---

## スキップするもの

| フェーズ | 担当エージェント | 理由 |
|----------|-----------------|------|
| 仕様書 Issue 作成・更新 | Product Manager | 小規模修正に仕様書更新は不要 |
| 設計方針策定 | Architect | アーキテクチャ影響なし |
| E2E テスト実行 | Tester | ビルド & ユニットテストで十分 |
| 完成判断 | Architect | 不要 |

---

## エスカレーション

以下の場合はユーザーに判断を求める:

- ビルド / テストが 3 回連続で失敗した
- コードレビューが 3 回連続で REQUEST CHANGES となった
- 修正範囲が当初の想定より大きくなった（feature-lifecycle への切り替えを提案）
- 修正内容が仕様やアーキテクチャに影響する可能性がある

---

## 参照スキル

- `github-flow` — Conventional Commits・ブランチ運用
- `dotnet-best-practices` — .NET コーディング規約
- `csharp-async` — 非同期プログラミング（該当する場合）
