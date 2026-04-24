# データアクセス戦略

> 対象: イベント参加登録システム DB アクセス基盤方針（SPEC: [event-registration-system-spec.md](./event-registration-system-spec.md) §関連 / Issue: [#5](https://github.com/runceel/ai-dev-dotnetapp/issues/5)）
> ステータス: **方針ドキュメント策定（Phase 3）**。本 SPEC のスコープは方針文書化のみであり、`DbContext` / リポジトリ実装は後続 SPEC で扱う。

---

## 概要

本ドキュメントは、本リポジトリ（イベント参加登録システム）における **DB アクセス方式の公式方針** を定義する。サンプル／開発／自動テスト用途のため **EF Core + `Microsoft.EntityFrameworkCore.InMemory`** を採用し、各業務モジュール（`Events` / `Registrations` 等）の Infrastructure 層に `DbContext` / リポジトリを実装する際の前提となる。アーキテクチャ全体像は [architecture.md](./architecture.md) を参照。

| 項目 | 内容 |
|------|------|
| 対象 | サンプル／開発／自動テスト用途の DB アクセス基盤 |
| 採用技術 | Entity Framework Core + `Microsoft.EntityFrameworkCore.InMemory` プロバイダ |
| 配置レイヤー | 各業務モジュールの `<Module>.Infrastructure` プロジェクト |
| 本番運用 | **不可**（InMemory プロバイダは本番用途で使用しない） |
| 参照元 SPEC | [Issue #5](https://github.com/runceel/ai-dev-dotnetapp/issues/5) |
| 関連ドキュメント | [architecture.md](./architecture.md) / [event-registration-system-spec.md](./event-registration-system-spec.md) |

<!-- trace: REQ-001, REQ-004, REQ-005, AC-005, AC-009 -->

---

## 1. 目的とスコープ

### Purpose

本リポジトリにおける DB アクセス方式の選定理由・適用パターン・制限事項・将来の拡張方針を **公式ドキュメント** として明文化し、後続のモジュール実装（`Events` / `Registrations` 等）で永続化を導入する際の判断基準を統一する。Clean Architecture × Modular Monolith のレイヤー依存方向（Domain ← Application ← Infrastructure）を保ったまま EF Core を導入するパターンを提示する。

### In Scope（本ドキュメントで扱う）

- DB アクセス方式として **EF Core + InMemoryDatabase** を採用するという方針の文書化
- 採用理由（サンプル用途・外部 DB 不要・CI/CD で扱いやすい等）の明記
- 各業務モジュールの Infrastructure レイヤーに `DbContext` を配置する方針
- Composition Root（`EventRegistration.Web/Program.cs`）における DI 登録の方針
- InMemoryDatabase の制限事項の明示
- 将来的に本番用 DB（Cosmos DB 等）へ切り替える際の拡張ポイントの記述

### Out of Scope（本ドキュメントでは扱わない）

- 実際の `DbContext` クラス／エンティティ／リポジトリの実装コード
- マイグレーション戦略（InMemory はマイグレーション非対応）
- 本番 DB（Cosmos DB / SQL Server / PostgreSQL 等）の実装・接続文字列・インフラ設定
- データシード（サンプルデータ投入）の具体的実装
- 認可・監査ログ等のクロスカッティング関心事

### 対象読者

本リポジトリで業務モジュールを実装する開発者・エージェント（Architect / Developer）、および将来 DB を本番用に切り替える担当者。

### 前提

- ランタイムは .NET 10、ホスティングは .NET Aspire、UI は Blazor Server。
- 現状リポジトリには EF Core 関連パッケージは **未導入** で、各モジュールの `Infrastructure` プロジェクトは `.gitkeep` のみの空状態である（[architecture.md §1](./architecture.md) 参照）。
- Modular Monolith の各モジュールは独立した `DbContext` を持ちうる（モジュール境界 = データ境界）。

<!-- trace: REQ-001, REQ-004, CON-001 -->

---

## 2. 採用方針（Policy Summary）

本プロジェクトの DB アクセス基盤として **EF Core + `Microsoft.EntityFrameworkCore.InMemory`** を採用する（**POL-001**）。これは「サンプル／開発／自動テスト用途で外部 DB を必要とせず、Clean Architecture のレイヤー依存方向を維持したまま、後続の本番 DB 切替に備える」という本リポジトリの位置づけに最も整合する選択である。

REQ-002 が要求する 5 セクション（採用方針／採用理由／適用パターン／制限事項／将来の拡張）の起点となる本章では、以下の中核方針を宣言する。

| 方針 ID | 内容 |
|---------|------|
| POL-001 | サンプル／開発／自動テスト用途の DB アクセス方式として **EF Core + InMemoryDatabase** を採用する |
| 配置先 | 各業務モジュールの `<Module>.Infrastructure` プロジェクト |
| 登録方法 | モジュールごとの DI 拡張メソッド (`AddXxxModuleInfrastructure`) を Composition Root から呼び出す（PAT-001） |
| 適用範囲 | 本番運用は **対象外**（CON-003 / §9 参照） |

<!-- trace: POL-001, REQ-002, AC-002 -->

---

## 3. 採用理由（Rationale）

| 観点 | 採用理由 |
|------|---------|
| **学習・サンプル用途** | 本リポジトリは "ai-dev-dotnetapp" として AI 駆動開発のリファレンス的位置づけであり、外部 DB セットアップなしで完結する体験を優先する |
| **開発体験** | `dotnet run` / Aspire AppHost 起動だけで動作し、開発者ローカルでの追加セットアップ（コンテナ起動・接続文字列設定）が不要 |
| **CI/CD 親和性** | GitHub Actions 等での実行時に外部リソースが不要で、ビルド・テストが安定して高速 |
| **EF Core 抽象の温存** | 後で SQL Server / PostgreSQL / Cosmos DB に切り替える際、`DbContext` ／ `DbSet` ／ LINQ クエリのコードはそのまま流用しやすい |
| **Clean Architecture 整合** | `DbContext` を Infrastructure に閉じ込める方針と相性が良く、Domain / Application を汚染しない |

### なぜ「ドキュメント策定」を独立 SPEC とするか

- 方針を先に明文化することで、後続のモジュール実装 SPEC が共通の前提を参照できる。
- コード変更を伴わないため低リスクで合意形成可能。
- 将来の本番 DB 移行時の判断材料（採用時の前提・限界）が記録として残る。

<!-- trace: REQ-002, AC-002 -->

---

## 4. 用語定義

| 用語 | 定義 |
|------|------|
| EF Core | Entity Framework Core。.NET 公式の ORM |
| InMemoryDatabase | EF Core プロバイダの一つ（`Microsoft.EntityFrameworkCore.InMemory`）で、プロセスメモリ上にデータを保持する。永続化なし |
| `DbContext` | EF Core の作業単位。エンティティ集合 (`DbSet`) を保持しクエリ・追跡・保存を提供する |
| Composition Root | DI コンテナへのサービス登録を行う唯一の場所。本リポジトリでは `EventRegistration.Web/Program.cs` |
| Modular Monolith | 単一プロセスでデプロイするが内部的に複数の自律モジュールに分割するアーキテクチャスタイル |
| Clean Architecture | Domain を最内に置き、依存方向を内向きに統一するレイヤード設計 |
| 業務モジュール | `Events` / `Registrations` 等、ドメイン境界ごとに分割されたモジュール群（`SharedKernel` を除く） |

<!-- trace: REQ-001, REQ-003 -->

---

## 5. レイヤー責務とアーキテクチャ整合（適用パターン①）

Clean Architecture の依存方向（Domain ← Application ← Infrastructure）を維持するため、EF Core 関連の参照は **Infrastructure 層に厳密に閉じ込める**。本方針は [architecture.md](./architecture.md) の **CON-007**（`SharedKernel.Domain` は参照ゼロ）／**CON-008**（業務モジュール間の直接参照禁止）と完全に整合する。

| レイヤー | EF Core 参照 | 責務 |
|----------|-------------|------|
| `<Module>.Domain` | **禁止**（POL-007） | エンティティ・値オブジェクト・ドメインサービス・ドメイン例外。EF Core を含む一切のフレームワーク非依存 |
| `<Module>.Application` | **抽象のみ依存**（POL-006） | UseCase / アプリケーションサービス。永続化はリポジトリインターフェース等の **抽象** を介して行い、`DbContext` 具象型に依存しない |
| `<Module>.Infrastructure` | **許可** | `DbContext` 派生クラス、リポジトリ実装、DI 登録拡張メソッドを配置 |

### 補足

- 本方針は [architecture.md §3.3](./architecture.md) の依存グラフと一貫している（`<Module>.Application` は `<Module>.Domain` のみを参照、`<Module>.Infrastructure` は `<Module>.Domain` / `<Module>.Application` を参照）。
- リポジトリインターフェースの配置は `<Module>.Application`（または `<Module>.Domain`）に置くことを推奨。具象実装を `<Module>.Infrastructure` に置くことで、依存方向が内側を向く（**CON-002**：既存依存方向と矛盾しない）。
- DI 登録は **Infrastructure 層** に配置する（POL-003）。`Application` 層は DI 登録先候補から除外し、Domain・Application が EF Core を一切参照しない不変条件（POL-007）と完全に整合させる。

<!-- trace: POL-006, POL-007, CON-002, CON-007, AC-005, AC-008 -->

---

## 6. DbContext 配置方針（適用パターン②）

「**1 モジュール = 1 (以上の) `DbContext`**」を原則とする（**POL-002** / **PAT-002**）。モジュール境界はそのままデータ境界となる。

### 配置ルール

| 項目 | 内容 |
|------|------|
| 配置プロジェクト | `<Module>.Infrastructure`（例: `EventRegistration.Events.Infrastructure`） |
| クラス名規約 | `<Module>DbContext`（例: `EventsDbContext` / `RegistrationsDbContext`）（推奨） |
| InMemory 名指定 | `UseInMemoryDatabase("<モジュール名>")` でモジュール単位に **一意** な名前を用いる（**POL-005** / AC-010） |
| 例 | `UseInMemoryDatabase("Events")` / `UseInMemoryDatabase("Registrations")` |

### 命名指針（POL-005）

InMemory プロバイダは「**同一プロセス内で同名の DB を共有する**」挙動を持つ。モジュール間で名前が衝突すると、別モジュールの `DbSet` が同じ内部ストアにマッピングされる事故が起こり得る。これを防ぐため、**モジュール名と一致する文字列**（または `"<ModuleName>-<Suffix>"` 形式）を必ず用いる。テスト用途で並列実行時のリーク防止が必要な場合は §11 のガイドラインに従う。

<!-- trace: POL-002, POL-005, PAT-002, AC-006, AC-010 -->

---

## 7. DI 登録パターン（適用パターン③）

DI 登録は、各モジュールの `<Module>.Infrastructure` プロジェクトに定義する **`AddXxxModuleInfrastructure(IServiceCollection)` 拡張メソッド** に集約し、Composition Root（`EventRegistration.Web/Program.cs`）からのみ呼び出す（**POL-003** / **POL-004** / **PAT-001**）。これは既存の UI Shell ナビゲーション登録方針（`AddEventsModuleNavigation` / `AddRegistrationsModuleNavigation`）と一貫した方式である。

### 規約

| ルール | 内容 |
|--------|------|
| 拡張メソッド配置 | `<Module>.Infrastructure` プロジェクト（例: `EventsModuleInfrastructureExtensions.AddEventsModuleInfrastructure`） |
| Composition Root の振る舞い | `builder.Services.AddXxxModuleInfrastructure();` を **呼ぶのみ**。`AddDbContext` の直書きは **禁止**（POL-004） |
| `UseInMemoryDatabase` 呼び出し位置 | 拡張メソッド内 **1 箇所のみ** に閉じ込める（**GUD-001**：将来の本番 DB 切替を 1 箇所の変更で完結させる差替え点） |
| `DbContext` の寿命 | **スコープド**（**GUD-004** / EF Core / DI のデフォルトに従う） |
| Application 層からの登録 | **行わない**（POL-007 と整合） |

### 期待される呼び出し関係（イメージ）

```
EventRegistration.Web/Program.cs (Composition Root)
        │
        ├─ builder.Services.AddEventsModuleInfrastructure();
        │       └─ services.AddDbContext<EventsDbContext>(o => o.UseInMemoryDatabase("Events"));
        │
        └─ builder.Services.AddRegistrationsModuleInfrastructure();
                └─ services.AddDbContext<RegistrationsDbContext>(o => o.UseInMemoryDatabase("Registrations"));
```

このパターンにより、(a) モジュールごとの DI 登録が **拡張メソッド境界** に閉じ込められ、(b) `UseXxx` を差し替えるだけで本番 DB 切替が可能になり（§10）、(c) Composition Root はモジュール詳細を知らずに済む。

<!-- trace: POL-003, POL-004, PAT-001, GUD-001, GUD-004, AC-007 -->

---

## 8. コード例（最小サンプル）

> **Note**: 以下のコード例は **設計パターンの図示を目的とした擬似コード** であり、本 SPEC のスコープでは実装しない。実際の `DbContext` / リポジトリ／DI 拡張メソッドの実装、および `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.InMemory` パッケージの追加は、後続の実装 SPEC で扱う（CON-001 整合）。

### 8.1 モジュール側 DI 拡張メソッド（イメージ）

```csharp
// EventRegistration.Events.Infrastructure/EventsModuleInfrastructureExtensions.cs
public static class EventsModuleInfrastructureExtensions
{
    public static IServiceCollection AddEventsModuleInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "Events"));

        // リポジトリ実装の登録（Application 層インターフェースに対する Infrastructure 実装の束ね）
        // services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
```

### 8.2 Composition Root 側の呼び出し（イメージ）

```csharp
// EventRegistration.Web/Program.cs
builder.Services.AddEventsModuleInfrastructure();
builder.Services.AddRegistrationsModuleInfrastructure();
```

> 上記コードは **本 SPEC のスコープでは実装しない**（CON-001）。後続 SPEC で本ドキュメントを参照しつつ実装する。

<!-- trace: PAT-001, CON-001 -->

---

## 9. 制限事項（InMemory の限界）

EF Core InMemoryDatabase プロバイダは、**本番用途では使用しない**ことが公式ガイダンスで明示されている（**CON-003**）。リレーショナル機能のサポートは部分的・限定的であり、本番 DB との挙動差を理解した上で利用する必要がある（**CON-004**）。

| 制限項目 | 内容 |
|----------|------|
| **本番運用** | **不可**。永続化なし／プロセス再起動でデータ消失（DAT-001）／個人情報保護等の要件下では不適格（COM-001） |
| **トランザクション** | `IDbContextTransaction` を完全サポートしない。`SaveChanges` の原子性に依存しないテスト設計が必要 |
| **外部キー制約** | 違反検出されない。リレーショナル制約に依存するテストは別プロバイダ（SQLite in-memory 等）を将来検討 |
| **Raw SQL** | `FromSqlRaw` / `ExecuteSqlRaw` 等の Raw SQL 実行は非対応 |
| **マイグレーション** | 非対応。`Database.Migrate` 不可 |
| **スキーマ管理** | `Database.EnsureCreated` のみ呼び出し可（実質 no-op） |
| **クエリ挙動の差異** | 一部 LINQ クエリ（大文字小文字比較・文字列処理等）が本番 DB と異なる場合がある |

InMemory プロバイダの公式ガイダンスは [Microsoft Learn / EF Core InMemory Provider](https://learn.microsoft.com/ef/core/providers/in-memory/) を参照。

<!-- trace: CON-003, CON-004, REQ-002, AC-002, AC-003 -->

---

## 10. 将来の拡張ポイント（本番 DB への切り替え）

§7 の DI 登録パターン（PAT-001）に従っている限り、本番 DB への切り替えは **拡張メソッド内の `UseXxx` 呼び出し 1 行の差し替え** で完了する（**GUD-002**）。`DbContext` ／ `DbSet` ／ LINQ クエリのコードはそのまま流用可能。

### 10.1 切替例（Cosmos DB / SQL Server）

```csharp
// 切替前: InMemoryDatabase
services.AddDbContext<EventsDbContext>(options =>
    options.UseInMemoryDatabase(databaseName: "Events"));

// 切替後（例: Cosmos DB）
services.AddDbContext<EventsDbContext>(options =>
    options.UseCosmos(connectionString, databaseName: "Events"));

// 切替後（例: SQL Server）
services.AddDbContext<EventsDbContext>(options =>
    options.UseSqlServer(connectionString));
```

> `Use` 呼び出し 1 行の差し替えで本番化できることを示す。**PAT-001 の拡張メソッド境界が、単一の差し替え点となる**。

### 10.2 切替時に追加で必要な検討事項

ただし、DI 登録の差替えは **主要な切替点に過ぎず**、実際にはプロバイダ固有の追加調整が発生し得る。これらは本 SPEC のスコープ外であり、本番 DB 切替時に別 SPEC として扱う:

- キー戦略・パーティションキー設計（特に Cosmos DB）
- トランザクション境界の見直し（マルチドキュメント／分散トランザクション）
- マイグレーション運用（`dotnet ef migrations` の導入）
- コネクション／リトライ設定・接続文字列管理（Aspire のリソース構成等）
- セキュリティ（認証方式・接続暗号化・最小権限）

利用可能な EF Core プロバイダ一覧は [Microsoft Learn / Database Providers](https://learn.microsoft.com/ef/core/providers/) を参照。

<!-- trace: GUD-002, REQ-002, AC-002, AC-004 -->

---

## 11. テスト方針

MSTest 4.x でリポジトリ単体テストを記述する際は、以下のガイドラインに従う（**GUD-003**）。具体的なテストコード実装は **本 SPEC のスコープ外** であり、後続の実装 SPEC で扱う（CON-001）。

| ガイドライン | 内容 |
|-------------|------|
| **テストごとに一意な DB 名** | 並列実行時の状態リークを防ぐため、`Guid.NewGuid().ToString()` 等を `UseInMemoryDatabase` の引数に用いる |
| **トランザクション依存禁止** | `IDbContextTransaction` を前提とするロジックは InMemory ではテストできない（§9 参照）。SUT から DI 越しに `DbContext` を解決し、`SaveChanges` 後の状態を直接アサートする方式を推奨 |
| **FK 制約に依存しない** | 外部キー違反は検出されないため、リレーショナル整合性をテストしたい場合は別プロバイダ（SQLite in-memory 等）の併用を将来検討 |
| **DI 寿命** | `DbContext` は **スコープド**（GUD-004）。テスト内では `IServiceScopeFactory` を介してスコープを切り、本番に近い寿命挙動を再現 |

CI/CD 連携については、将来 `Microsoft.EntityFrameworkCore.InMemory` を導入する際、MSTest によるリポジトリ単体テストで InMemoryDatabase を利用する構成を本ドキュメントに追記する。

<!-- trace: GUD-003, CON-001 -->

---

## 12. 関連仕様・参考リンク

- **SPEC Issue**: [#5 — architecture: EF Core InMemoryDatabase をサンプル用 DB アクセス方式として採用する方針](https://github.com/runceel/ai-dev-dotnetapp/issues/5)
- **アーキテクチャ全体像**: [architecture.md](./architecture.md)（特に [§関連ドキュメント](./architecture.md#関連ドキュメント) / [§2.2 モジュール系プロジェクト（共通パターン）](./architecture.md#22-モジュール系プロジェクト共通パターン) / [§3.3 依存方向](./architecture.md) / [§5 設計上の制約](./architecture.md#5-設計上の制約--不変条件サマリ)）
- **システム仕様**: [event-registration-system-spec.md](./event-registration-system-spec.md)
- **UI Shell 設計**: [ui-shell-design.md](./ui-shell-design.md)
- **Developer エージェント定義**: [.github/agents/developer.agent.md](../.github/agents/developer.agent.md)（POL-008 により本ドキュメントへの参照を保持）
- **EF Core InMemory プロバイダ公式ドキュメント**: https://learn.microsoft.com/ef/core/providers/in-memory/
- **EF Core プロバイダ一覧**（将来の本番切替候補）: https://learn.microsoft.com/ef/core/providers/

<!-- trace: REQ-004, REQ-005, AC-005, AC-009, POL-008 -->

---

## 改訂履歴

| 日付 | 変更内容 | 関連 Issue/PR |
|------|---------|---------------|
| 2026-04-24 | 初版作成（SPEC #5） | #5 / #6 |