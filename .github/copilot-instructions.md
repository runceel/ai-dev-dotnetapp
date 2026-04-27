# Copilot 開発ワークフローガイド

本リポジトリでは、要件定義から開発・テストまでを Copilot スキルで支援します。
以下のフェーズ順にスキルを活用してください。

---

## フェーズ 1: 要件理解・分析

| スキル | 用途 |
|--------|------|
| `office-document-analyzer` | Excel / PowerPoint 等の設計書を解析して内容を読み取る |

- `docs/user-stories/` 配下の Excel 設計書を `office-document-analyzer` で解析し、要件を把握する

## フェーズ 2: アーキテクチャ・仕様策定

| スキル | 用途 |
|--------|------|
| `create-specification` | AI 向けに最適化された仕様書を GitHub Issue として作成する |
| `cosmosdb-datamodeling` | Cosmos DB のデータモデルを設計する |
| `clean-architecture-guide` | Modular Monolith + Clean Architecture の設計ガイドに従う |

- 要件をもとに `create-specification` で GitHub Issue として仕様書化する
- データ層の設計が必要な場合は `cosmosdb-datamodeling` を使用する
- モジュール構成・レイヤー設計は `clean-architecture-guide` に従う
- 実装計画は仕様書 Issue にコメントとして記録する（`feature-lifecycle` スキルの Phase 2 参照）

## フェーズ 3: 開発

| スキル | 用途 |
|--------|------|
| `github-flow` | **必須** — feature ブランチ作成から Draft PR、Squash Merge までの開発フロー |
| `dotnet-best-practices` | .NET / C# のベストプラクティスに準拠する |
| `aspnet-minimal-api-openapi` | ASP.NET Minimal API + OpenAPI のエンドポイント実装 |
| `aspire` | Aspire による分散アプリケーションの構成・実行・デバッグ |
| `cosmosdb-datamodeling` | Azure Cosmos DB のデータモデル設計（`docs/cosmosdb_data_model.md` 参照） |
| `csharp-async` | C# 非同期プログラミングのベストプラクティス |
| `microsoft-docs` | Microsoft 公式ドキュメントの参照 |
| `microsoft-code-reference` | Microsoft API リファレンス・コードサンプルの検索 |

- **コードを変更する際は必ず `github-flow` に従う**（feature ブランチ → Draft PR → レビュー → Squash Merge）
- .NET / C# コードは `dotnet-best-practices` に準拠する
- 最新の .NET 10 / C# 14 / ASP.NET Core 10 / Blazor 10 の機能は `dotnet10` / `aspnetcore-blazor10` スキルで確認する

### ライフサイクルスキルの使い分け

`feature-lifecycle` スキルは GitHub Issue / PR を正本として使用する。
`cloud-agent-lifecycle` スキルは GitHub Cloud Agent の readonly 制約下で、仕様・状態台帳・Agent 間ログ・PR 本文案を `.github/lifecycle/<work-id>/` 配下に保存する。
ユーザーがどちらかのスキルを明示した場合は、その transport 方針を優先する。GitHub Cloud Agent 上で `gh` CLI または GitHub API が readonly かつユーザーがファイルベース運用を求めている場合は、`cloud-agent-lifecycle` を使用する。

## フェーズ 4: テスト

| スキル | 用途 |
|--------|------|
| `csharp-mstest` | MSTest によるユニットテストの作成 |
| Playwright MCP server / `playwright-cli` | ブラウザを使った E2E テストの自動化。Playwright MCP server が提供されている場合は MCP を優先し、未提供時のみ `playwright-cli` にフォールバックする |

- E2E テストやブラウザ操作では、利用可能なツール一覧に Playwright MCP server 由来のツールがある場合、`playwright-cli` より MCP ツールを優先する
- Playwright MCP server が未提供、または MCP ツールでは必要な操作を実行できない場合のみ、`playwright-cli` / `e2e-test` スキルの CLI 手順にフォールバックする

---

## 仕様変更時のフロー

仕様に変更が発生した場合は、以下のスキルで差分を反映する。

1. `update-specification` — 既存の仕様書 Issue を更新する

---

## ユーティリティスキル

| スキル | 用途 |
|--------|------|
| `scripting-guide` | Python 等の小規模スクリプトでデータ処理を行う場合のガイド |
| `documentation-guide` | **必須** — Markdown ドキュメント作成・更新時の共通ルール |

- **ドキュメントを作成・更新する際は必ず `documentation-guide` に従う**

---

## Windows + PowerShell 環境ルール

本リポジトリは Windows + PowerShell 環境での開発を前提とする。
日本語テキストを扱う際はエンコーディングに注意すること。

### `gh` CLI で日本語を含む PR / Issue を編集する場合

`gh pr edit --body-file` や `gh issue edit --body-file` で日本語テキストを送ると、
PowerShell のデフォルトエンコーディング（Shift_JIS）により文字化けが発生する。

以下の手順で **必ず UTF-8 BOM 付き** で書き出すこと:

```powershell
# 1. コンソールを UTF-8 に切り替え
chcp 65001 > $null
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 2. here-string で本文を作成
$body = @"
## 概要
...
"@

# 3. UTF-8 BOM 付きでファイルに書き出し
$tempFile = [System.IO.Path]::GetTempFileName()
[System.IO.File]::WriteAllText($tempFile, $body, [System.Text.UTF8Encoding]::new($true))

# 4. gh CLI で適用 → 一時ファイル削除
gh pr edit <番号> --body-file $tempFile
Remove-Item $tempFile
```

> `Set-Content` や `Out-File` は使用しないこと。エンコーディング制御が不確実なため。

---

## カスタムエージェント

本リポジトリには 7 つのカスタムエージェント（`.github/agents/`）が定義されている。

| エージェント | 役割 |
|---|---|
| **Orchestrator** | ライフサイクル全体の駆動・状態管理・エージェント委譲 |
| **Product Manager** | 仕様書 Issue 作成・要件管理（`new` / `draft` 両モード対応） |
| **Architect** | 設計レビュー・完成判断（COMPLETE / INCOMPLETE） |
| **Developer** | 実装・ユニットテスト・PR 作成・指摘反映 |
| **Reviewer** | 仕様/コードレビュー（APPROVE / REQUEST CHANGES） |
| **Tester** | E2E テスト実行・バグ報告（PASS / FAIL） |
| **Documentation** | 設計ドキュメント骨子生成・実装追従 docs 更新（DONE / UP_TO_DATE） |

### 自律開発フロー

`feature-lifecycle` スキルと Orchestrator エージェントを組み合わせることで、仕様策定→実装→テスト→ドキュメント更新→マージの全サイクルを自律的に進行できる。人間が既にドラフト Issue を作成している場合は `draft` エントリモードで指定することで、新規 Issue を作らずにそのまま SPEC 化できる。仕様確定時（Step 1.5）とマージ前（Step 3.11）に人間の承認を挟む。

---

## 典型的な 1 機能の開発サイクル

```
要件分析 → 仕様書 Issue 作成 → 設計・実装計画（Issue コメント）
→ github-flow でブランチ作成 → 実装 → テスト → PR → レビュー → マージ
```
