namespace EventRegistration.SharedKernel.Application.DemoData;

/// <summary>
/// 各モジュールがデモ用データを投入するためのシーダー抽象。
/// </summary>
/// <remarks>
/// 上位プロセス (Web 等) は <see cref="IDemoDataSeeder"/> を <see cref="IEnumerable{T}"/>
/// として解決し、<see cref="Order"/> 昇順に <see cref="SeedAsync"/> を呼び出す。
/// 各実装は冪等性を保証する責務を負う (既存データがある場合は何もしない)。
/// </remarks>
public interface IDemoDataSeeder
{
    /// <summary>
    /// 実行順序。値が小さい方を先に実行する。
    /// モジュール間の依存関係 (例: Registrations は Events に依存) を表現する。
    /// </summary>
    int Order { get; }

    /// <summary>
    /// デモ用データを投入する。既存データがある場合は何もしない。
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken);
}
