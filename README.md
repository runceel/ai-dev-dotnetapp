# EventRegistration

> イベント参加登録システム — .NET 10 / Aspire / Blazor Server (MudBlazor) / Modular Monolith × Clean Architecture

---

## 概要

イベントを作成し、参加者が登録できるイベント参加登録システム。
Modular Monolith × Clean Architecture を採用し、モジュール境界をプロジェクト境界として物理的に強制している。
.NET Aspire により AppHost からの起動と観測性（OpenTelemetry / ヘルスチェック）を最初から確保する Walking Skeleton をベースに、
業務モジュール（Events / Registrations / Notifications）と MudBlazor ベースの UI Shell を順次追加している。

| 項目 | 内容 |
|---|---|
| ランタイム | .NET 10（`net10.0`）|
| SDK バージョン | `10.0.201`（`global.json` でピン留め、`rollForward: latestFeature`）|
| アーキテクチャ | Modular Monolith + Clean Architecture（モジュール単位適用）|
| 業務モジュール | `SharedKernel` / `Events` / `Registrations` / `Notifications`（各 Domain / Application / Infrastructure の 3 プロジェクト）|
| ホスト系 | `AppHost`（.NET Aspire）/ `ServiceDefaults` / `Web`（Blazor Web App: Server interactivity / MudBlazor 9.4.0）|
| テスト | `EventRegistration.Web.Tests`（bUnit による Web コンポーネントテスト）|
| プロジェクト総数 | 16（ホスト 3 + モジュール 12 + テスト 1）|
| ソリューション形式 | クラシック `.sln`（リポジトリルート）|
| ライセンス | MIT（[LICENSE](./LICENSE)）|

詳細な設計は [`docs/architecture.md`](./docs/architecture.md) を、機能仕様は [`docs/event-registration-system-spec.md`](./docs/event-registration-system-spec.md) を参照。

---

## 主な機能

- **Events**: イベントの作成・一覧表示・詳細表示
- **Registrations**: 参加登録（定員超過時はキャンセル待ち）・キャンセル（先頭の繰り上げ）・参加者一覧
- **Notifications**: 参加確定・繰り上げ等の重要なドメインイベントを購読し、ログ出力で通知（`INotificationSender` 差し替えで本番拡張可能）
- **UI Shell**: MudBlazor ベースの共通レイアウトと、各モジュールが提供する `INavigationItem` の Self-Registration によるサイドナビゲーション

---

## 前提条件

- **.NET 10 SDK**（`10.0.201` 以上のフィーチャーバンド。`global.json` により自動的に該当バージョンが選択される）
- **.NET Aspire 13** に対応した `Aspire.AppHost.Sdk`（プロジェクト復元時に自動取得）
- **HTTPS 開発証明書**（初回のみ）:

  ```powershell
  dotnet dev-certs https --trust
  ```

- HTTP クライアント（ブラウザ / `curl` / `Invoke-WebRequest`）

SDK バージョン確認:

```powershell
dotnet --list-sdks       # 10.0.x が存在することを確認
dotnet --version         # global.json により 10.0.201 以上が選択される想定
```

---

## ビルド

リポジトリルートで:

```powershell
dotnet restore EventRegistration.sln
dotnet build EventRegistration.sln
```

期待結果: **エラー 0**。

---

## テスト

```powershell
dotnet test EventRegistration.sln
```

`src/tests/EventRegistration.Web.Tests` の bUnit ベースのコンポーネントテストおよび統合テストが実行される。
テスト方針の詳細は [`docs/tests/web-component-tests.md`](./docs/tests/web-component-tests.md) を参照。

---

## 起動

```powershell
dotnet run --project src/EventRegistration.AppHost
```

起動後、標準出力に Aspire ダッシュボードの URL が表示される（例: `Now listening on: https://localhost:17xxx`）。

| アクセス先 | 期待される表示 |
|---|---|
| ダッシュボード URL（`https://localhost:<dashboard-port>`）| Aspire ダッシュボードの Resources ページ。`web`（`EventRegistration.Web`）が `Running` で表示される |
| Web リソースのエンドポイント URL | MudBlazor ベースのホーム画面と、Events / Registrations のナビゲーション項目 |

停止は `Ctrl+C`。

---

## プロジェクト構成

```
ai-dev-dotnetapp/
├── EventRegistration.sln
├── global.json
├── LICENSE
├── README.md
├── docs/
│   ├── architecture.md
│   ├── data-access-strategy.md
│   ├── event-registration-system-spec.md
│   ├── ui-shell-design.md
│   ├── modules/
│   │   ├── shared-kernel.md
│   │   ├── events.md
│   │   ├── registrations.md
│   │   └── notifications.md
│   └── tests/
│       └── web-component-tests.md
└── src/
    ├── EventRegistration.AppHost/             # Aspire オーケストレーション
    ├── EventRegistration.ServiceDefaults/     # 観測性・ヘルスチェック共通設定
    ├── EventRegistration.Web/                 # Blazor Web App + MudBlazor + Composition Root
    ├── Modules/
    │   ├── SharedKernel/
    │   │   ├── EventRegistration.SharedKernel.Domain/
    │   │   ├── EventRegistration.SharedKernel.Application/
    │   │   └── EventRegistration.SharedKernel.Infrastructure/
    │   ├── Events/
    │   │   ├── EventRegistration.Events.Domain/
    │   │   ├── EventRegistration.Events.Application/
    │   │   └── EventRegistration.Events.Infrastructure/
    │   ├── Registrations/
    │   │   ├── EventRegistration.Registrations.Domain/
    │   │   ├── EventRegistration.Registrations.Application/
    │   │   └── EventRegistration.Registrations.Infrastructure/
    │   └── Notifications/
    │       ├── EventRegistration.Notifications.Domain/
    │       ├── EventRegistration.Notifications.Application/
    │       └── EventRegistration.Notifications.Infrastructure/
    └── tests/
        └── EventRegistration.Web.Tests/       # bUnit による Web コンポーネント / 統合テスト
```

参照グラフと依存方向の詳細は [`docs/architecture.md`](./docs/architecture.md) を参照。

---

## 新しいモジュールの追加

`docs/architecture.md` の手順に従う。要点:

1. `src/Modules/<NewModuleName>/` 配下に Domain / Application / Infrastructure の 3 プロジェクトを `dotnet new classlib -f net10.0` で作成
2. `dotnet sln EventRegistration.sln add <csproj> --solution-folder Modules/<NewModuleName>` で登録
3. `<NewModuleName>.Domain` から `SharedKernel.Domain` への参照を追加
4. レイヤー間参照（`Application → Domain` / `Infrastructure → Application + Domain`）を追加
5. `EventRegistration.Web` から `<NewModuleName>.Application` と `<NewModuleName>.Infrastructure` への参照を追加し、`Program.cs` で DI 拡張メソッド（`AddXxxModuleNavigation` / `AddXxxModuleInfrastructure` 等）を呼び出す
6. **他の業務モジュールへの直接参照は追加しない**（モジュール間の連携はドメインイベントまたは `EventRegistration.Web/Adapters/` 配下のアダプター経由）

---

## 検証コマンド

```powershell
# プロジェクト一覧（16 件）
dotnet sln EventRegistration.sln list

# SharedKernel.Domain は参照ゼロであるべき
dotnet list src/Modules/SharedKernel/EventRegistration.SharedKernel.Domain/EventRegistration.SharedKernel.Domain.csproj reference

# 業務モジュール間の直接参照が 0 件であること
Get-ChildItem -Recurse -Path src/Modules/Events -Filter *.csproj | Select-String -Pattern 'Registrations|Notifications'
Get-ChildItem -Recurse -Path src/Modules/Registrations -Filter *.csproj | Select-String -Pattern 'Events|Notifications'
Get-ChildItem -Recurse -Path src/Modules/Notifications -Filter *.csproj | Select-String -Pattern 'Events|Registrations'
```

---

## ドキュメント

| ドキュメント | 内容 |
|---|---|
| [docs/architecture.md](./docs/architecture.md) | アーキテクチャ設計（プロジェクト構成・参照方向・テスト戦略・リスク・AC トレーサビリティ）|
| [docs/data-access-strategy.md](./docs/data-access-strategy.md) | データアクセス方針（EF Core + InMemoryDatabase 採用方針・`DbContext` 配置・DI 拡張メソッドパターン）|
| [docs/event-registration-system-spec.md](./docs/event-registration-system-spec.md) | システム機能仕様（業務要件・画面構成）|
| [docs/ui-shell-design.md](./docs/ui-shell-design.md) | MudBlazor ベースの UI Shell 設計とナビゲーション Self-Registration 機構 |
| [docs/modules/shared-kernel.md](./docs/modules/shared-kernel.md) | SharedKernel モジュール仕様 |
| [docs/modules/events.md](./docs/modules/events.md) | Events モジュール仕様 |
| [docs/modules/registrations.md](./docs/modules/registrations.md) | Registrations モジュール仕様 |
| [docs/modules/notifications.md](./docs/modules/notifications.md) | Notifications モジュール仕様 |
| [docs/tests/web-component-tests.md](./docs/tests/web-component-tests.md) | Web コンポーネントテスト方針（bUnit）|

---

## ライセンス

本リポジトリは [MIT License](./LICENSE) の下で公開されています。
