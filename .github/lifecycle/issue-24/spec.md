# SPEC: デモ用シードデータの自動投入機能 (Issue #24)

## 背景・目的
クローン直後の起動でも「イベント一覧」「参加者一覧」「Analytics」画面に意味のあるデータが
表示される状態を作り、初学者・デモ・CI 検証時の UX を向上させる。

## スコープ
- Events / Registrations モジュール用のデモ用データセットを提供する。
- アプリケーション起動時に DbContext が空であれば自動投入する。
- 環境ごとに ON/OFF を切り替え可能にする。

## 機能要件
1. **デモデータセット**
   - Events: 過去/直近/将来の代表的な 3 件のイベント。定員は 2〜10 で多様性を持たせる。
   - Registrations: 各イベントに対して Confirmed / WaitListed の参加者を複数件投入。
2. **冪等性**
   - 各シーダーは対象 DbContext に既存レコードがある場合は **何もしない**。
   - 二重起動・再起動でデータが膨らまないこと。
3. **起動時投入**
   - `IHostedService` 派生 (`DemoDataHostedService`) で `StartAsync` 中に実行する。
   - シーダー間の順序を制御する (`Order`)。Events → Registrations の順。
4. **ON/OFF 設定**
   - `DemoData:Enabled` (bool) を `appsettings*.json` または環境変数で制御。
   - Development 環境では既定 ON、それ以外は既定 OFF。
   - 値が指定された場合は環境に関わらずその値を優先。
5. **失敗時の挙動**
   - シーダーが失敗してもアプリケーションの起動自体は継続させる (ログ出力のみ)。

## 非機能要件
- Modular Monolith / Clean Architecture を維持する。
- シーダーの実装は各モジュール `Infrastructure` 配下に閉じる。
- `IDemoDataSeeder` 抽象は `SharedKernel.Application` に置き、上位プロセス (Web) から
  モジュール非依存で列挙できるようにする。

## 受け入れ基準 (AC)
- AC-01: Development で `dotnet run` 後に `/events` にイベントが 3 件表示される。
- AC-02: 同じく `/events/{id}/registrations` 等で参加者一覧が表示される。
- AC-03: `DemoData:Enabled=false` を設定すると、データが投入されない。
- AC-04: 既存データがある状態でアプリを再起動しても、データが重複しない。
- AC-05: `dotnet build` / `dotnet test` がすべて成功する。

## 影響範囲
- `src/Modules/SharedKernel/EventRegistration.SharedKernel.Application/DemoData/IDemoDataSeeder.cs` (新規)
- `src/Modules/Events/EventRegistration.Events.Infrastructure/DemoData/EventsDemoDataSeeder.cs` (新規)
- `src/Modules/Registrations/EventRegistration.Registrations.Infrastructure/DemoData/RegistrationsDemoDataSeeder.cs` (新規)
- `src/EventRegistration.Web/DemoData/{DemoDataOptions,DemoDataHostedService}.cs` (新規)
- 既存: 各 `*ModuleInfrastructureExtensions.cs`、`EventRegistration.Web/Program.cs`、`appsettings.Development.json`
