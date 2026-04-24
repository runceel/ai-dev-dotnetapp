# EventRegistration

> イベント参加登録システム — .NET 10 / Aspire / Blazor Server / Modular Monolith × Clean Architecture

---

## 概要

本リポジトリは、イベント参加登録システムの開発リポジトリ。Phase 1（基盤プロジェクト構造の作成）として、空のソリューションと 12 プロジェクトの構造のみが整備されている状態である。業務機能（イベント作成・参加登録など）は未実装。

| 項目 | 内容 |
|---|---|
| ランタイム | .NET 10（`net10.0`）|
| SDK バージョン | `10.0.203`（`global.json` でピン留め、`rollForward: latestPatch`）|
| アーキテクチャ | Modular Monolith + Clean Architecture（モジュール単位適用）|
| モジュール | `SharedKernel` / `Events` / `Registrations`（各 Domain / Application / Infrastructure の 3 プロジェクト）|
| ホスト系 | `AppHost`（.NET Aspire）/ `ServiceDefaults` / `Web`（Blazor Web App: Server interactivity / Empty）|
| プロジェクト総数 | 12（ホスト 3 + モジュール 9）|
| ソリューション形式 | クラシック `.sln`（リポジトリルート）|

詳細な設計は [`docs/architecture.md`](./docs/architecture.md) を、機能仕様は [`docs/event-registration-system-spec.md`](./docs/event-registration-system-spec.md) を参照。

---

## 前提条件

- **.NET 10 SDK**（`10.0.203` 以上のパッチ。`global.json` により自動的に該当バージョンが選択される）
- **.NET Aspire 13** に対応した `Aspire.AppHost.Sdk`（プロジェクト復元時に自動取得）
- **HTTPS 開発証明書**（初回のみ）:

  ```powershell
  dotnet dev-certs https --trust
  ```

- HTTP クライアント（ブラウザ / `curl` / `Invoke-WebRequest`）

SDK バージョン確認:

```powershell
dotnet --list-sdks       # 10.0.x が存在することを確認
dotnet --version         # global.json により 10.0.203 が選択される想定
```

---

## ビルド

リポジトリルートで:

```powershell
dotnet restore EventRegistration.sln
dotnet build EventRegistration.sln
```

期待結果: **エラー 0 / 警告 0**（CON-006 / AC-001）。

---

## 起動

```powershell
dotnet run --project src/EventRegistration.AppHost
```

起動後、標準出力に Aspire ダッシュボードの URL が表示される（例: `Now listening on: https://localhost:17xxx`）。

| アクセス先 | 期待される表示 |
|---|---|
| ダッシュボード URL（`https://localhost:<dashboard-port>`）| Aspire ダッシュボードの Resources ページ。`web`（`EventRegistration.Web`）が `Running` で表示される |
| Web リソースのエンドポイント URL | Blazor Web App（Empty テンプレート）の Home ページ |

停止は `Ctrl+C`。

---

## プロジェクト構成

```
ai-dev-dotnetapp/
├── EventRegistration.sln
├── global.json
├── docs/
│   ├── architecture.md
│   ├── data-access-strategy.md
│   └── event-registration-system-spec.md
└── src/
    ├── EventRegistration.AppHost/             # Aspire オーケストレーション
    ├── EventRegistration.ServiceDefaults/     # 観測性・ヘルスチェック共通設定
    ├── EventRegistration.Web/                 # Blazor Web App + Composition Root
    └── Modules/
        ├── SharedKernel/
        │   ├── EventRegistration.SharedKernel.Domain/
        │   ├── EventRegistration.SharedKernel.Application/
        │   └── EventRegistration.SharedKernel.Infrastructure/
        ├── Events/
        │   ├── EventRegistration.Events.Domain/
        │   ├── EventRegistration.Events.Application/
        │   └── EventRegistration.Events.Infrastructure/
        └── Registrations/
            ├── EventRegistration.Registrations.Domain/
            ├── EventRegistration.Registrations.Application/
            └── EventRegistration.Registrations.Infrastructure/
```

参照グラフと依存方向の詳細は [`docs/architecture.md` §3](./docs/architecture.md#3-プロジェクト参照方向依存グラフ) を参照。

---

## 新しいモジュールの追加

`docs/architecture.md` §11 の手順に従う。要点:

1. `src/Modules/<NewModuleName>/` 配下に Domain / Application / Infrastructure の 3 プロジェクトを `dotnet new classlib -f net10.0` で作成
2. `dotnet sln EventRegistration.sln add <csproj> --solution-folder Modules/<NewModuleName>` で登録
3. `<NewModuleName>.Domain` から `SharedKernel.Domain` への参照を追加
4. レイヤー間参照（`Application → Domain` / `Infrastructure → Application + Domain`）を追加
5. `EventRegistration.Web` から `<NewModuleName>.Application` と `<NewModuleName>.Infrastructure` への参照を追加
6. **他の業務モジュールへの直接参照は追加しない**（CON-008）

---

## 検証コマンド

`docs/architecture.md` §12.5 のコマンド集を参照。主要なもの:

```powershell
# プロジェクト一覧（12 件）
dotnet sln EventRegistration.sln list

# SharedKernel.Domain は参照ゼロであるべき（CON-007）
dotnet list src/Modules/SharedKernel/EventRegistration.SharedKernel.Domain/EventRegistration.SharedKernel.Domain.csproj reference

# 業務モジュール間の直接参照が 0 件であること（CON-008）
Get-ChildItem -Recurse -Path src/Modules/Events -Filter *.csproj | Select-String -Pattern 'Registrations'
Get-ChildItem -Recurse -Path src/Modules/Registrations -Filter *.csproj | Select-String -Pattern 'Events'
```

---

## ドキュメント

| ドキュメント | 内容 |
|---|---|
| [docs/architecture.md](./docs/architecture.md) | アーキテクチャ設計（プロジェクト構成・参照方向・テスト戦略・リスク・AC トレーサビリティ）|
| [docs/data-access-strategy.md](./docs/data-access-strategy.md) | データアクセス方針（EF Core + InMemoryDatabase 採用方針・`DbContext` 配置・DI 拡張メソッドパターン）|
| [docs/event-registration-system-spec.md](./docs/event-registration-system-spec.md) | システム機能仕様（業務要件・将来実装予定）|

---

## ライセンス

未定。
