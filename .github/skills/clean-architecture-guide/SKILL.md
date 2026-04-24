---
name: clean-architecture-guide
description: Modular Monolith + Clean Architecture の設計ガイド。新しいモジュールの作成、画面の実装、機能追加を行う際には、必ずこのスキルを参照して従ってください。
---

# Clean Architecture 設計ガイド

このスキルは、本プロジェクトにおける Modular Monolith + Clean Architecture の具体的な設計パターンとルールを定義します。

> **重要:** このスキルに基づいてコード編集・生成を行う際には、対応する言語・プラットフォームのスキル（例: `dotnet10`、`aspnetcore-blazor10`、`aspire` 等）を必ず併せて参照してください。最新の言語機能や API を正しく使用するために必要です。

## 前提: .NET Aspire によるオーケストレーション

本プロジェクトは **.NET Aspire** を使用してアプリケーションの構成・実行・監視を行います。

- **AppHost プロジェクト** がすべてのサービスとインフラリソース（DB、キャッシュ等）を一元管理します
- ローカル開発は `aspire run` で全リソースを起動し、Aspire Dashboard でログ・トレース・メトリクスを確認します
- デプロイは `aspire publish` でマニフェストを生成し、Azure Container Apps 等にデプロイします
- サービス間の接続は Aspire のサービスディスカバリ（`.WithReference()`）を通じて自動的に解決されます

```
src/
  AppHost/                          # Aspire AppHost（オーケストレーション）
  ServiceDefaults/                  # 共通のサービス設定（テレメトリ、ヘルスチェック等）
  Web/                              # Blazor Server フロントエンド
  Modules/<ModuleName>/             # 各モジュール（Clean Architecture 3 層）
```

> **Aspire の詳細な API やパターンについては `aspire` スキルを参照してください。**

## 1. モジュール構成

新しいモジュールは以下の 3 プロジェクトで構成します。

```
src/Modules/<ModuleName>/
  <ModuleName>.Domain/           # エンティティ、値オブジェクト（依存なし）
  <ModuleName>.Application/      # UseCase、DTO、リポジトリインターフェース（Domain のみ依存）
  <ModuleName>.Infrastructure/   # リポジトリ実装、外部サービス連携、DI 登録（Application に依存）
```

### プロジェクト作成手順

```bash
# 1. プロジェクト作成
dotnet new classlib -n <ModuleName>.Domain -o src/Modules/<ModuleName>/<ModuleName>.Domain --framework net10.0
dotnet new classlib -n <ModuleName>.Application -o src/Modules/<ModuleName>/<ModuleName>.Application --framework net10.0
dotnet new classlib -n <ModuleName>.Infrastructure -o src/Modules/<ModuleName>/<ModuleName>.Infrastructure --framework net10.0

# 2. ソリューションに追加
dotnet sln add src/Modules/<ModuleName>/<ModuleName>.Domain
dotnet sln add src/Modules/<ModuleName>/<ModuleName>.Application
dotnet sln add src/Modules/<ModuleName>/<ModuleName>.Infrastructure

# 3. プロジェクト参照設定
dotnet add src/Modules/<ModuleName>/<ModuleName>.Application reference src/Modules/<ModuleName>/<ModuleName>.Domain
dotnet add src/Modules/<ModuleName>/<ModuleName>.Infrastructure reference src/Modules/<ModuleName>/<ModuleName>.Application

# 4. Infrastructure に DI パッケージ追加
dotnet add src/Modules/<ModuleName>/<ModuleName>.Infrastructure package Microsoft.Extensions.DependencyInjection.Abstractions

# 5. Infrastructure に Azure Cosmos DB SDK パッケージ追加
dotnet add src/Modules/<ModuleName>/<ModuleName>.Infrastructure package Microsoft.Azure.Cosmos

# 6. Web からの参照追加
dotnet add src/Web reference src/Modules/<ModuleName>/<ModuleName>.Application
dotnet add src/Web reference src/Modules/<ModuleName>/<ModuleName>.Infrastructure

# 7. 自動生成された Class1.cs を各プロジェクトから削除
```

---

## 2. 依存関係ルール

```
Web ──→ Application (UseCase インターフェース、DTO のみ)
Web ──→ Infrastructure (DI 登録の拡張メソッド呼び出しのためのみ)

Infrastructure ──→ Application (リポジトリインターフェースの実装)
Application    ──→ Domain      (エンティティの利用)
Domain         ──→ (依存なし)
```

### 禁止事項

| ルール | 説明 |
|--------|------|
| **Web から Domain エンティティを直接参照しない** | Web 層は Application 層の DTO のみを使用する |
| **Web から Repository を直接注入しない** | Web 層は UseCase インターフェース経由でのみデータにアクセスする |
| **ビジネスロジックを Web / Infrastructure に書かない** | ビジネスロジックは Domain と Application のみに配置する |
| **Application 層から Infrastructure を参照しない** | Application 層はインターフェースを定義し、実装は Infrastructure 層が行う |
| **リポジトリ内で個別の永続化呼び出しは不要** | Cosmos DB SDK の各操作（CreateItemAsync, ReplaceItemAsync 等）は即座に永続化される。EF Core のような SaveChangesAsync パターンは不要 |

---

## 3. 各層の実装パターン

### 3.1 Domain 層 — エンティティ

ビジネスの中核となるデータ構造を定義します。

```csharp
namespace <ModuleName>.Domain;

public class <EntityName>
{
    public int ID { get; set; }
    public string Name { get; set; } = "";
    // ビジネスルールに関わるメソッドもここに配置可能
}
```

### 3.2 Application 層 — DTO

Web 層に公開するデータ転送オブジェクトです。Domain エンティティの代わりにこれを使います。

```csharp
namespace <ModuleName>.Application;

public record <EntityName>Dto(int ID, string Name);
```

### 3.3 Application 層 — リポジトリインターフェース

データアクセスの抽象を定義します。UseCase 内部から使用され、Web 層には公開しません。

```csharp
namespace <ModuleName>.Application;

public interface I<EntityName>Repository
{
    Task<IReadOnlyList<Domain.<EntityName>>> GetAllAsync();
    Task UpdateAsync(int id, ...);
}
```

### 3.4 Application 層 — IUnitOfWork インターフェース（オプション）

同一パーティション内で複数操作をアトミックに実行する必要がある場合のみ使用します。Cosmos DB SDK の各操作（`CreateItemAsync` 等）は個別に即座永続化されるため、単一操作の UseCase では不要です。

```csharp
namespace <ModuleName>.Application;

/// <summary>
/// 同一パーティション内の複数操作をアトミックに実行する場合に使用。
/// Cosmos DB の TransactionalBatch に対応する。
/// 単一操作の UseCase では不要。
/// </summary>
public interface IUnitOfWork
{
    Task ExecuteBatchAsync(CancellationToken cancellationToken = default);
}
```

> **ポイント:** Cosmos DB では個別の CRUD 操作が即座に永続化されるため、EF Core のような `SaveChangesAsync` パターンは不要です。`IUnitOfWork` は同一パーティション内の TransactionalBatch が必要な場合のみ使用します。

### 3.5 Application 層 — UseCase

**Web 層が唯一依存する操作インターフェース** です。1 つの操作 = 1 つの UseCase とし、`ExecuteAsync` メソッドを持ちます。

```csharp
namespace <ModuleName>.Application.UseCases;

// インターフェース
public interface IGet<EntityName>sUseCase
{
    Task<IReadOnlyList<<EntityName>Dto>> ExecuteAsync(string? searchText = null);
}

// 実装（Application 層内に配置）
public class Get<EntityName>sUseCase(I<EntityName>Repository repository) : IGet<EntityName>sUseCase
{
    public async Task<IReadOnlyList<<EntityName>Dto>> ExecuteAsync(string? searchText = null)
    {
        var entities = await repository.GetAllAsync();

        // フィルタリングなどのビジネスロジックはここで行う
        var filtered = string.IsNullOrEmpty(searchText)
            ? entities
            : entities.Where(e => e.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

        // Domain → DTO 変換
        return filtered.Select(e => new <EntityName>Dto(e.ID, e.Name)).ToList();
    }
}
```

```csharp
namespace <ModuleName>.Application.UseCases;

public interface IUpdate<EntityName>UseCase
{
    Task ExecuteAsync(int id, ...);
}

public class Update<EntityName>UseCase(I<EntityName>Repository repository) : IUpdate<EntityName>UseCase
{
    public async Task ExecuteAsync(int id, ...)
    {
        await repository.UpdateAsync(id, ...);
        // Cosmos DB SDK では各操作が即座に永続化されるため、SaveChangesAsync は不要
    }
}
```

### 3.6 Infrastructure 層 — Cosmos DB コンテナプロバイダー

データアクセスには **Azure Cosmos DB SDK（Microsoft.Azure.Cosmos）** を直接使用します。モジュールが使用する Cosmos DB コンテナへのアクセスを提供するプロバイダークラスを定義します。

```csharp
using Microsoft.Azure.Cosmos;

namespace <ModuleName>.Infrastructure;

/// <summary>
/// モジュールが使用する Cosmos DB コンテナへのアクセスを提供する。
/// </summary>
public class <ModuleName>CosmosContainers
{
    public Container <ContainerName> { get; }

    public <ModuleName>CosmosContainers(CosmosClient cosmosClient, string databaseName)
    {
        var database = cosmosClient.GetDatabase(databaseName);
        <ContainerName> = database.GetContainer("<container-name>");
    }
}
```

> **ポイント:** Cosmos DB SDK の各操作（`CreateItemAsync`, `ReadItemAsync`, `ReplaceItemAsync`, `DeleteItemAsync`）は即座に永続化されます。EF Core のような変更追跡や `SaveChangesAsync` パターンは不要です。

### 3.7 Infrastructure 層 — リポジトリ実装

リポジトリは `<ModuleName>CosmosContainers` を注入し、Cosmos DB SDK を使ってデータアクセスを行います。各操作は即座に永続化されます。

```csharp
using <ModuleName>.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace <ModuleName>.Infrastructure;

public class <EntityName>Repository(<ModuleName>CosmosContainers containers) : Application.I<EntityName>Repository
{
    public async Task<IReadOnlyList<<EntityName>>> GetAllAsync()
    {
        var query = containers.<ContainerName>
            .GetItemLinqQueryable<<EntityName>>()
            .Where(e => e.Type == "<entity-type>")
            .ToFeedIterator();

        var results = new List<<EntityName>>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task UpdateAsync(string id, string partitionKey, ...)
    {
        var response = await containers.<ContainerName>.ReadItemAsync<<EntityName>>(
            id, new PartitionKey(partitionKey));
        var item = response.Resource;
        // プロパティの更新
        await containers.<ContainerName>.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
        // Cosmos DB では ReplaceItemAsync で即座に永続化される
    }
}
```

### 3.8 Infrastructure 層 — DI 登録

モジュール単位で `Add<ModuleName>Module()` 拡張メソッドを提供します。

```csharp
using <ModuleName>.Application;
using <ModuleName>.Application.UseCases;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace <ModuleName>.Infrastructure;

public static class <ModuleName>ServiceCollectionExtensions
{
    public static IServiceCollection Add<ModuleName>Module(this IServiceCollection services, CosmosClient cosmosClient, string databaseName)
    {
        // Cosmos DB コンテナプロバイダー
        services.AddSingleton(new <ModuleName>CosmosContainers(cosmosClient, databaseName));

        // Infrastructure（リポジトリ実装）
        services.AddScoped<I<EntityName>Repository, <EntityName>Repository>();

        // Application（UseCase）
        services.AddScoped<IGet<EntityName>sUseCase, Get<EntityName>sUseCase>();
        services.AddScoped<IUpdate<EntityName>UseCase, Update<EntityName>UseCase>();

        return services;
    }
}
```

---

## 4. Web 層（Blazor ページ）の実装パターン

Web 層は **UseCase インターフェースと DTO のみ** を使用します。

```razor
@page "/<route>"
@using <ModuleName>.Application
@using <ModuleName>.Application.UseCases
@rendermode InteractiveServer

<PageTitle>ページタイトル</PageTitle>

<h1>ページタイトル</h1>

@* UI コンポーネント *@

@code {
    [Inject]
    private IGet<EntityName>sUseCase Get<EntityName>sUseCase { get; set; } = default!;

    [Inject]
    private IUpdate<EntityName>UseCase Update<EntityName>UseCase { get; set; } = default!;

    private IReadOnlyList<<EntityName>Dto> items = [];

    protected override async Task OnInitializedAsync()
    {
        items = await Get<EntityName>sUseCase.ExecuteAsync();
    }
}
```

### Program.cs での登録（Aspire 連携）

Web プロジェクトでは Aspire ServiceDefaults を追加し、各モジュールの DI 登録を行います。
接続文字列は Aspire AppHost の `.WithReference()` によって自動注入されます。

```csharp
using <ModuleName>.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults（テレメトリ、ヘルスチェック等）
builder.AddServiceDefaults();

// モジュール登録（CosmosClient は Aspire 経由で注入、DB 名は設定から取得）
var cosmosClient = new CosmosClient(builder.Configuration.GetConnectionString("cosmos")!);
builder.Services.Add<ModuleName>Module(cosmosClient, "{projectname}-db");

var app = builder.Build();

// Aspire デフォルトエンドポイント（ヘルスチェック等）
app.MapDefaultEndpoints();
```

### AppHost での Web プロジェクト登録

AppHost の `Program.cs` で Web プロジェクトとインフラリソースを接続します。

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// インフラリソース
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .AddDatabase("{projectname}-db");
var cache = builder.AddRedis("cache");

// Web アプリケーション
builder.AddProject<Projects.Web>("web")
    .WithReference(cosmos)
    .WithReference(cache)
    .WaitFor(cosmos);

builder.Build().Run();
```

> **ポイント:** モジュールが使用する Cosmos DB やキャッシュなどのインフラリソースは AppHost で定義し、`.WithReference()` で Web プロジェクトに渡します。Web の `Program.cs` では `builder.Configuration.GetConnectionString()` で自動的にその接続情報を取得できます。Cosmos DB のデータベース名・コンテナ名は各モジュールの DI 登録で指定します。

---

## 5. テスト実装ルール

新しいモジュールや機能を実装する際は、**単体テスト**と**インテグレーションテスト**の両方を必ず作成してください。

> **MSTest の詳細なベストプラクティスは `csharp-mstest` スキルを参照してください。**
> **Aspire Testing の詳細な API は `aspire` スキルの [Testing リファレンス](../aspire/references/testing.md) を参照してください。**

### 5.1 テストプロジェクト構成

```
tests/
  <ModuleName>.Application.Tests/     # 単体テスト（UseCase、ドメインロジック）
  AppHost.IntegrationTests/           # インテグレーションテスト（Aspire AppHost 経由）
```

### 5.2 単体テスト（モジュールごと）

UseCase とドメインロジックの単体テストを **モジュールごとに** 作成します。

#### プロジェクト作成

```bash
# 1. テストプロジェクト作成
dotnet new mstest -n <ModuleName>.Application.Tests -o tests/<ModuleName>.Application.Tests --framework net10.0

# 2. ソリューションに追加
dotnet sln add tests/<ModuleName>.Application.Tests

# 3. テスト対象への参照追加
dotnet add tests/<ModuleName>.Application.Tests reference src/Modules/<ModuleName>/<ModuleName>.Application
```

#### テストの書き方

- **対象**: Application 層の UseCase 実装、Domain 層のビジネスロジック
- **リポジトリはモックする**: `I<EntityName>Repository` と `IUnitOfWork` をモック/スタブで差し替え
- **命名規則**: `MethodName_Scenario_ExpectedBehavior`
- **クラスは sealed**: パフォーマンスと設計明確化のため

```csharp
using <ModuleName>.Application.UseCases;
using <ModuleName>.Domain;

namespace <ModuleName>.Application.Tests;

[TestClass]
public sealed class Get<EntityName>sUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsync_WithSearchText_ReturnsFilteredResults()
    {
        // Arrange
        var repository = new Fake<EntityName>Repository([
            new <EntityName> { ID = 1, Name = "Alpha" },
            new <EntityName> { ID = 2, Name = "Beta" },
        ]);
        var useCase = new Get<EntityName>sUseCase(repository);

        // Act
        var result = await useCase.ExecuteAsync("Alpha");

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Alpha", result[0].Name);
    }
}
```

### 5.3 インテグレーションテスト（Aspire AppHost 経由）

Aspire `Hosting.Testing` を使い、**AppHost から全リソースを起動**して End-to-End に近いテストを行います。

#### プロジェクト作成（初回のみ）

```bash
# 1. テストプロジェクト作成
dotnet new mstest -n AppHost.IntegrationTests -o tests/AppHost.IntegrationTests --framework net10.0

# 2. ソリューションに追加
dotnet sln add tests/AppHost.IntegrationTests

# 3. AppHost への参照と Aspire Testing パッケージ追加
dotnet add tests/AppHost.IntegrationTests reference src/AppHost
dotnet add tests/AppHost.IntegrationTests package Aspire.Hosting.Testing
```

#### csproj に `IsAspireTestProject` を追加

```xml
<PropertyGroup>
  <IsAspireTestProject>true</IsAspireTestProject>
</PropertyGroup>
```

#### テストの書き方

- **対象**: サービスのヘルスチェック、API エンドポイント、DB を含む一連のフロー
- **`DistributedApplicationTestingBuilder`** で AppHost を丸ごと起動
- **`await using`** で確実にクリーンアップ
- **タイムアウト**: コンテナ起動があるため余裕を持つ（2 分推奨）

```csharp
using System.Net;
using Aspire.Hosting.Testing;

namespace AppHost.IntegrationTests;

[TestClass]
public sealed class HealthCheckTests
{
    [TestMethod]
    [Timeout(120_000)]
    public async Task Web_HealthEndpoint_ReturnsOk()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await builder.BuildAsync();
        await app.StartAsync();
        await app.WaitForResourceReadyAsync("web");

        // Act
        var client = app.CreateHttpClient("web");
        var response = await client.GetAsync("/health");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

### 5.4 テスト実装の原則

| ルール | 説明 |
|--------|------|
| **単体テストはモジュール単位** | 各モジュールの Application.Tests プロジェクトに配置 |
| **インテグレーションテストは AppHost 単位** | AppHost.IntegrationTests に集約 |
| **テストは独立** | テスト間で状態を共有しない。各テストが自己完結すること |
| **リポジトリをモックする** | 単体テストでは DB に依存しない。Fake/Mock/Stub を使用 |
| **実インフラで検証する** | インテグレーションテストでは Aspire が起動する実コンテナ（DB、キャッシュ等）を使用 |
| **PR 前に全テスト通過** | `dotnet test {ProjectName}.slnx` が成功することを確認してから PR を作成（github-flow 参照） |

---

## 6. チェックリスト

新しいモジュールや機能を実装する際に確認してください。

- [ ] Domain 層にエンティティを定義したか
- [ ] Application 層に DTO を定義したか
- [ ] Application 層にリポジトリインターフェースを定義したか
- [ ] Application 層に IUnitOfWork インターフェースを定義したか
- [ ] Application 層に UseCase（インターフェース + 実装）を定義したか
- [ ] Infrastructure 層に Cosmos DB コンテナプロバイダーを定義したか
- [ ] Infrastructure 層にリポジトリ実装を配置したか（Cosmos DB SDK を使用し、各操作は即座に永続化される）
- [ ] 複数操作のアトミック実行が必要な場合に TransactionalBatch（IUnitOfWork）を使用しているか
- [ ] Infrastructure 層に DI 登録用の拡張メソッドを作成したか
- [ ] Web 層は UseCase と DTO のみを参照しているか（Domain エンティティを直接参照していないか）
- [ ] Program.cs に `Add<ModuleName>Module()` を追加したか
- [ ] AppHost でモジュールが必要とするインフラリソース（DB 等）を定義し、`.WithReference()` で Web に接続しているか
- [ ] Web の Program.cs に `builder.AddServiceDefaults()` と `app.MapDefaultEndpoints()` を追加しているか
- [ ] `dotnet build` が成功するか
- [ ] `aspire run` で正常に起動し、Aspire Dashboard でサービスが確認できるか
- [ ] 単体テスト（`<ModuleName>.Application.Tests`）を作成したか
- [ ] インテグレーションテスト（`AppHost.IntegrationTests`）にヘルスチェック等のテストを追加したか
- [ ] `dotnet test {ProjectName}.slnx` が全件成功するか
