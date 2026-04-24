---
name: github-flow
description: 'このリポジトリでの開発フロー（GitHub Flow）を定義するスキル。feature ブランチの作成から Draft PR、レビュー、Squash Merge までの一連の作業手順とルールを示します。コードを変更する作業を行う場合は、必ずこのスキルに従ってください。'
---

# GitHub Flow 開発ガイド

このリポジトリでは **GitHub Flow** を採用しています。すべてのコード変更は、以下のフローに従って行ってください。

---

## フロー概要

```
main（保護ブランチ）
 │
 ├─ feature/<issue番号>-<短い説明> ブランチを作成
 │   ├─ 作業・コミット（Conventional Commits）
 │   ├─ dotnet build / dotnet test で品質確認
 │   └─ Draft PR を作成
 │       ├─ 作業完了後 → Ready for Review に変更
 │       ├─ レビュー実施
 │       └─ Squash Merge → main へ統合
 │
 └─ ブランチ削除
```

---

## 1. ブランチの作成

`main` ブランチから feature ブランチを切ります。**直接 `main` にコミットしてはいけません。**

### ブランチ命名規則

```
feature/<issue番号>-<短い説明>
```

| 例 | 説明 |
|---|------|
| `feature/42-add-ai-service` | Issue #42 に対応する AI サービス追加 |
| `feature/15-fix-chat-scroll` | Issue #15 のチャットスクロール不具合修正 |

### コマンド例

```bash
git switch main
git pull origin main
git switch -c feature/42-add-ai-service
```

---

## 2. コミット規約（Conventional Commits）

すべてのコミットメッセージは [Conventional Commits](https://www.conventionalcommits.org/ja/) に従います。

### フォーマット

```
<type>(<scope>): <description>

[本文（任意）]

[フッター（任意）]
```

### type 一覧

| type | 用途 |
|------|------|
| `feat` | 新機能の追加 |
| `fix` | バグ修正 |
| `docs` | ドキュメントのみの変更 |
| `style` | コードの意味に影響しない変更（空白、フォーマット等） |
| `refactor` | バグ修正でも機能追加でもないコード変更 |
| `test` | テストの追加・修正 |
| `chore` | ビルドプロセスや補助ツールの変更 |

### scope の例

このリポジトリでは以下の scope を推奨します。

| scope | 対象 |
|-------|------|
| `aichat` | AIChat モジュール全般 |
| `aichat-domain` | AIChat.Domain プロジェクト |
| `aichat-app` | AIChat.Application プロジェクト |
| `aichat-infra` | AIChat.Infrastructure プロジェクト |
| `web` | Web プロジェクト（Blazor UI） |
| `skill` | GitHub Copilot スキル |

### コミットメッセージの例

```
feat(aichat): Azure OpenAI サービスとの連携を追加

SendMessageUseCase に Azure OpenAI クライアントを統合し、
スタブ応答を実際の AI 応答に置き換えた。

Refs #42
```

```
fix(web): チャットメッセージの自動スクロールが効かない問題を修正

Refs #15
```

---

## 3. ビルド・テスト・テスト作成の確認

PR を作成する**前**に、必ずローカルでビルドとテストを実行して成功を確認してください。

```bash
cd src
dotnet build {ProjectName}.slnx
dotnet test {ProjectName}.slnx
```

> **注意:** ビルドエラーやテスト失敗がある状態で PR を作成しないでください。

### テスト作成の義務

新規・変更したビジネスロジックには **単体テストを必ず作成** してください。

- テストの書き方 → `csharp-mstest` スキルを参照
- テストプロジェクトの配置規約 → `clean-architecture-guide` スキルを参照
- 単体テストは `<ModuleName>.Application.Tests` プロジェクトに配置する
- DTO・インターフェースのみの変更など、テスト対象のロジックがない場合はテスト不要

---

## 4. Draft PR の作成

feature ブランチをリモートにプッシュし、**Draft Pull Request** を作成します。

```bash
git push -u origin feature/42-add-ai-service
```

### PR タイトル

Conventional Commits と同じ形式にします。

```
feat(aichat): Azure OpenAI サービスとの連携を追加
```

### PR 本文テンプレート

```markdown
## 概要
<!-- 変更の目的と背景を簡潔に記述 -->

## 変更内容
<!-- 主な変更点を箇条書きで記述 -->
-
-

## 関連 Issue
<!-- 対応する Issue を記載 -->
Closes #<issue番号>

## 確認事項
- [ ] `dotnet build {ProjectName}.slnx` が成功する
- [ ] `dotnet test {ProjectName}.slnx` が成功する
- [ ] 新規・変更したビジネスロジックに単体テストがある（`csharp-mstest` スキル参照）
- [ ] テストプロジェクトの配置が規約に従っている（`clean-architecture-guide` スキル参照）
```

### Draft PR として作成する理由

- 作業中であることをチームに共有できる
- 早い段階でフィードバックを得られる
- CI が走り、問題を早期検出できる

---

## 5. レビュー依頼

作業が完了したら以下を行います。

1. PR のステータスを **Ready for Review** に変更する
2. レビュアーをアサインする
3. 必要に応じてコメントで補足説明を追加する

---

## 6. レビューとマージ

### レビュアーの作業

- コードの品質・設計を確認する
- 必要に応じてコメントや修正リクエストを行う
- 問題がなければ Approve する

### マージ方法

**Squash and Merge** を使用します。

- feature ブランチの複数コミットが 1 つのコミットにまとまる
- `main` ブランチの履歴がクリーンに保たれる
- マージコミットのメッセージは PR タイトルを使用する

### マージ後

- feature ブランチは**削除**する（GitHub の自動削除設定を推奨）
- ローカルの不要ブランチも整理する

```bash
git switch main
git pull origin main
git branch -d feature/42-add-ai-service
```

---

## 禁止事項

| ❌ やってはいけないこと | ✅ 代わりにやること |
|---|---|
| `main` ブランチに直接コミット | feature ブランチを作成してから作業 |
| ビルドが通らない状態で PR 作成 | `dotnet build` / `dotnet test` を先に実行 |
| Force push を `main` に実行 | feature ブランチでのみ必要に応じて使用 |
| レビューなしでマージ | 必ずレビュー承認を経てからマージ |
| Merge commit や Rebase merge | Squash Merge を使用 |

---

## クイックリファレンス

```bash
# 1. ブランチ作成
git switch main && git pull origin main
git switch -c feature/<issue番号>-<説明>

# 2. 作業・コミット
git add .
git commit -m "feat(<scope>): 変更内容の説明"

# 3. ビルド確認
cd src && dotnet build {ProjectName}.slnx && dotnet test {ProjectName}.slnx

# 4. プッシュ & Draft PR 作成
git push -u origin feature/<issue番号>-<説明>
# GitHub で Draft PR を作成

# 5. 完了後 → Ready for Review → レビュー → Squash Merge

# 6. マージ後のクリーンアップ
git switch main && git pull origin main
git branch -d feature/<issue番号>-<説明>
```
