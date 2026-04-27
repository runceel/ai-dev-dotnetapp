using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using EventRegistration.SharedKernel.Application.DemoData;

namespace EventRegistration.Web.DemoData;

/// <summary>
/// Events モジュール用のデモデータ投入シーダー。
/// 既存のイベントが 1 件でも存在する場合は何もしない (冪等性)。
/// </summary>
public sealed class EventsDemoDataSeeder(IEventRepository eventRepository) : IDemoDataSeeder
{
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var existing = await eventRepository.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            Event.Create(
                name: ".NET 10 リリース記念ミートアップ",
                description: ".NET 10 / C# 14 の新機能を紹介するハンズオン形式の勉強会です。",
                scheduledAt: now.AddDays(14),
                capacity: 30),
            Event.Create(
                name: "Blazor もくもく会",
                description: "Blazor を使った個人開発・業務開発の経験を共有しながら、もくもく作業します。",
                scheduledAt: now.AddDays(30),
                capacity: 8),
            Event.Create(
                name: "Aspire ライトニングトーク大会",
                description: ".NET Aspire を使った分散アプリ開発についての LT 大会。発表者・聴講者ともに歓迎。",
                scheduledAt: now.AddDays(-7),
                capacity: 50),
            Event.Create(
                name: "C# ハンズオンワークショップ",
                description: "C# 初心者〜中級者を対象とした実践的なコーディングワークショップ。",
                scheduledAt: now.AddDays(7),
                capacity: 15),
            Event.Create(
                name: "クラウドネイティブ勉強会",
                description: "Azure / AWS / GCP を横断的に比較し、クラウドネイティブ設計のベストプラクティスを共有。",
                scheduledAt: now.AddDays(-3),
                capacity: 20),
            Event.Create(
                name: "AI × .NET 最新動向セミナー",
                description: "Semantic Kernel や ML.NET を活用した AI 統合の最前線を紹介するセミナー。",
                scheduledAt: now.AddDays(21),
                capacity: 40),
            Event.Create(
                name: "OSS コントリビュート入門",
                description: "初めての OSS コントリビュートを体験するハンズオン。GitHub の使い方から PR の出し方まで。",
                scheduledAt: now.AddDays(-14),
                capacity: 12),
        };

        foreach (var ev in events)
        {
            await eventRepository.AddAsync(ev, cancellationToken);
        }
    }
}
