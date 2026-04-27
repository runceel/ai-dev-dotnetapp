---
name: e2e-test
description: 'Playwright MCP server が提供されている場合は MCP を優先し、未提供時は playwright-cli で E2E テストを実行するスキル。ブラウザを headless モードで起動し、テスト結果を screenshots/ 配下の日付付きフォルダーに保存する。E2E テストを実行する際には、必ずこのスキルに従ってください。'
---

# E2E テストスキル

## 概要

本スキルは、Playwright MCP server または `playwright-cli` を使って {ProjectName} アプリケーションの E2E テストを実行し、テスト結果をスクリーンショットとして記録するための手順を定義する。
すべてのスクリーンショットは `screenshots/` 配下の日付付きフォルダーに保存される。

---

## ツール優先順位

1. 利用可能なツール一覧に Playwright MCP server 由来のブラウザ操作ツールがある場合は、MCP ツールを最優先で使用する
2. Playwright MCP server が未提供、接続不可、または必要な操作を MCP ツールで実行できない場合のみ、`playwright-cli` にフォールバックする
3. `playwright-cli` にフォールバックした場合は、テスト結果にフォールバック理由を簡潔に記録する

---

## 前提条件

- Aspire AppHost が起動済みであること（`cd src && dotnet run --project AppHost`）
- Web アプリケーションがアクセス可能であること
- Playwright MCP server または `playwright-cli` が利用可能であること

---

## 実行手順

### Step 1: スクリーンショット保存先の作成

テスト実行ごとに一意のフォルダーを `screenshots/` 配下に作成する。

**フォルダー命名規則:**

```
screenshots/{YYYYMMDD}-{HHmmss}-{テスト名}/
```

- `YYYYMMDD-HHmmss` — テスト開始時刻（ローカルタイム）
- `テスト名` — テストの目的を英語で簡潔に表す（kebab-case）

**例:**

```
screenshots/20260327-143052-chat-send-message/
screenshots/20260327-150000-data-management-upload/
screenshots/20260327-160000-admin-user-crud/
screenshots/20260327-170000-full-regression/
```

**作成コマンド（PowerShell）:**

```powershell
$testName = "chat-send-message"  # テストに応じて変更
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$folder = "screenshots/${timestamp}-${testName}"
New-Item -ItemType Directory -Path $folder -Force
```

### Step 2: ブラウザの起動（headless モード）

Playwright MCP server が利用可能な場合は、MCP のブラウザ起動/ページ遷移ツールを使用する。
以下の `playwright-cli` コマンドは、MCP が利用できない場合のフォールバック手順である。

```bash
playwright-cli open --browser=chromium
```

> **注意:** `playwright-cli` は現時点で headless フラグを直接サポートしていない場合がある。
> その場合は `run-code` で headless コンテキストを作成する:
>
> ```bash
> playwright-cli open
> ```

### Step 3: 画面サイズの設定

一貫したスクリーンショットのため、画面サイズを固定する。
Playwright MCP server 使用時は、同等の viewport / browser resize 操作を MCP ツールで実行する。

```bash
playwright-cli resize 1920 1080
```

### Step 4: テスト対象ページへの遷移

Web アプリケーションの URL に遷移する。Aspire 経由で起動している場合、URL はダッシュボードから確認する。
Playwright MCP server 使用時は、同等の navigate 操作を MCP ツールで実行する。

```bash
playwright-cli goto https://localhost:{ポート番号}
```

### Step 5: スクリーンショットの撮影

各操作ステップでスクリーンショットを撮影する。ファイル名は連番 + 操作内容とする。
Playwright MCP server 使用時は、同等の screenshot 操作を MCP ツールで実行し、指定ファイル名で保存する。

**命名規則:**

```
{連番2桁}-{操作内容}.png
```

**例:**

```bash
playwright-cli screenshot --filename=screenshots/20260327-143052-chat-send-message/01-initial-page.png
playwright-cli screenshot --filename=screenshots/20260327-143052-chat-send-message/02-new-session-created.png
playwright-cli screenshot --filename=screenshots/20260327-143052-chat-send-message/03-message-typed.png
playwright-cli screenshot --filename=screenshots/20260327-143052-chat-send-message/04-message-sent.png
playwright-cli screenshot --filename=screenshots/20260327-143052-chat-send-message/05-ai-response.png
```

### Step 6: テスト完了・ブラウザの終了

Playwright MCP server 使用時は、MCP のブラウザ終了/セッション終了操作を使用する。
`playwright-cli` フォールバック時は以下を実行する。

```bash
playwright-cli close
```

---

## テストシナリオテンプレート

以下は典型的なテストシナリオの実行例。テスト対象に応じてカスタマイズして使用する。

### チャット画面の基本テスト

```bash
# 1. 準備
$testName = "chat-basic"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$folder = "screenshots/${timestamp}-${testName}"
New-Item -ItemType Directory -Path $folder -Force

# 2. ブラウザ起動・遷移
playwright-cli open
playwright-cli resize 1920 1080
playwright-cli goto {WebアプリURL}/chat

# 3. テスト実行とスクリーンショット
playwright-cli screenshot --filename=${folder}/01-chat-initial.png

# 新規セッション作成
playwright-cli click {新規セッションボタンのref}
playwright-cli screenshot --filename=${folder}/02-new-session.png

# メッセージ送信
playwright-cli fill {入力欄のref} "テストメッセージ"
playwright-cli screenshot --filename=${folder}/03-message-typed.png
playwright-cli click {送信ボタンのref}

# AI 応答待ち（snapshot で状態確認しながら待つ）
playwright-cli snapshot
playwright-cli screenshot --filename=${folder}/04-response-received.png

# 4. 終了
playwright-cli close
```

### データ管理画面のテスト

```bash
$testName = "data-management"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$folder = "screenshots/${timestamp}-${testName}"
New-Item -ItemType Directory -Path $folder -Force

playwright-cli open
playwright-cli resize 1920 1080
playwright-cli goto {WebアプリURL}/data-management

playwright-cli screenshot --filename=${folder}/01-data-list.png

# 検索テスト
playwright-cli fill {検索欄のref} "テスト"
playwright-cli screenshot --filename=${folder}/02-search-result.png

playwright-cli close
```

### 管理画面（ユーザー管理）のテスト

```bash
$testName = "admin-users"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$folder = "screenshots/${timestamp}-${testName}"
New-Item -ItemType Directory -Path $folder -Force

playwright-cli open
playwright-cli resize 1920 1080
playwright-cli goto {WebアプリURL}/admin/users

playwright-cli screenshot --filename=${folder}/01-users-list.png

# ユーザー追加ダイアログ
playwright-cli click {追加ボタンのref}
playwright-cli screenshot --filename=${folder}/02-add-user-dialog.png

playwright-cli close
```

---

## ルール

### 必須

1. **headless モードで実行する** — GUI を表示しない
2. **Playwright MCP server が提供されている場合は MCP ツールを優先する** — CLI はフォールバック用途に限定する
3. **screenshots/ 配下に日付フォルダーを必ず作成する** — ルート直下にスクリーンショットを保存しない
4. **フォルダー名は `YYYYMMDD-HHmmss-テスト名` 形式** にする
5. **スクリーンショットのファイル名は `連番2桁-操作内容.png`** にする
6. **テスト前に画面サイズを `1920x1080` に設定する**
7. **テスト完了後は必ず MCP のセッション終了操作または `playwright-cli close` でブラウザを終了する**
8. **各操作ステップでスクリーンショットを撮影する** — 後からテスト結果を確認できるようにする

### 推奨

- テスト開始時に Aspire ダッシュボードのスクリーンショットを撮り、全サービスの状態を記録する
- エラー発生時もスクリーンショットを撮影してから対処する
- `snapshot` コマンドで DOM の状態を確認してから操作する（ref の特定に必要）
- 待機が必要な場合は `snapshot` を繰り返して状態遷移を確認する

### 禁止

- `screenshots/` ルート直下へのスクリーンショット保存
- タイムスタンプのないフォルダー名の使用
- テスト終了時にブラウザを閉じ忘れること

---

## トラブルシューティング

| 問題 | 対処法 |
|------|--------|
| Playwright MCP server のツールが見つからない | `playwright-cli` にフォールバックし、結果に理由を記録 |
| Playwright MCP server に接続できない | MCP server の起動/設定を確認。解消できなければ `playwright-cli` にフォールバック |
| ブラウザが起動しない | `playwright-cli kill-all` で残存プロセスを終了してから再試行 |
| ページが表示されない | Aspire ダッシュボードで Web アプリの起動状態を確認 |
| ref が見つからない | `playwright-cli snapshot` で最新の DOM 構造を確認 |
| スクリーンショットが保存されない | フォルダーが存在するか確認。`--filename` のパスが正しいか確認 |
| SSL 証明書エラー | 開発環境では `https://localhost` の自己署名証明書を許可する |
