# PR: デモ用シードデータの自動投入機能

Closes #24

## 概要

クローン直後・初回起動時に Events / Registrations モジュールへ意味のあるサンプルデータを
自動投入する仕組みを追加した。これにより、ローカル開発・デモ・CI のいずれでも
「イベント一覧」「参加者一覧」「Analytics」画面に最初からデータが表示される。

## 設計

```
EventRegistration.Web (Composition Root)
└─ DemoDataHostedService (IHostedService)
   ├─ DemoDataOptions { Enabled }
   └─ IEnumerable<IDemoDataSeeder>  (SharedKernel.Application 抽象)
        ├─ EventsDemoDataSeeder        (Order = 10)
        └─ RegistrationsDemoDataSeeder (Order = 20)
```

- `IDemoDataSeeder` は SharedKernel.Application に置き、モジュール非依存に列挙可能。
- 各シーダーは「対象 DbContext が空のときだけ」投入することで冪等性を確保。
- `DemoDataHostedService` は `Order` 昇順で実行し、シーダーの例外は WARN ログのみで
  起動を継続する。

## 環境ごとの ON/OFF

`appsettings*.json` または環境変数の `DemoData:Enabled` で制御。

| 環境 | 既定値 |
|------|--------|
| Development | `true` (`appsettings.Development.json` で明示 ON) |
| その他 (Production 等) | `false` |

設定セクション `DemoData` が存在しない場合は、`builder.Environment.IsDevelopment()` を
基に自動判定する (`Program.cs` の `PostConfigure`)。

## 投入されるデータ

- **Events**: 3 件
  - .NET 10 リリース記念ミートアップ (定員 30, 2 週間後)
  - Blazor もくもく会 (定員 5, 1 か月後)
  - Aspire ライトニングトーク大会 (定員 50, 1 週間前)
- **Registrations**: 各イベントについて 定員 + 1 名分 (上限 8) を順次登録。
  既存の `RegisterParticipantUseCase` を経由するため、定員に応じて自動的に
  Confirmed / WaitListed が振り分けられる。

## 変更ファイル

新規:
- `src/Modules/SharedKernel/EventRegistration.SharedKernel.Application/DemoData/IDemoDataSeeder.cs`
- `src/EventRegistration.Web/DemoData/EventsDemoDataSeeder.cs`
- `src/EventRegistration.Web/DemoData/RegistrationsDemoDataSeeder.cs`
- `src/EventRegistration.Web/DemoData/DemoDataOptions.cs`
- `src/EventRegistration.Web/DemoData/DemoDataHostedService.cs`
- `src/tests/EventRegistration.Web.Tests/DemoData/*.cs` (3 ファイル, 8 テスト)

変更:
- `src/EventRegistration.Web/Program.cs`
- `src/EventRegistration.Web/appsettings.Development.json`
- `README.md`

## 検証

- `dotnet build EventRegistration.slnx` → 成功
- `dotnet test EventRegistration.slnx` → **154 passed / 0 failed**

## 受け入れ基準

- [x] AC-01 / AC-02: シーダーが Events / Registrations に投入する経路をユニットテストで検証。
- [x] AC-03: `DemoDataHostedServiceTests.StartAsync_DoesNotInvokeSeeders_WhenDisabled`
- [x] AC-04: 各シーダーの `IsIdempotent` テストでカバー
- [x] AC-05: 全 154 テスト成功
