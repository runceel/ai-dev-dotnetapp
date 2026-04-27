using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.Events.Application.Repositories;
using EventRegistration.SharedKernel.Application.DemoData;

namespace EventRegistration.Web.DemoData;

/// <summary>
/// Analytics モジュール用のデモデータ投入シーダー。
/// 日別チャートが見栄えよく表示されるよう、過去 14 日間にわたる
/// <see cref="RegistrationActivity"/> を直接投入する。
/// 既に Activity が 1 件でも存在する場合は何もしない (冪等性)。
/// </summary>
/// <remarks>
/// このシーダーは Events / Registrations シーダーの後に実行する (<see cref="Order"/> = 30)。
/// Events シーダーが作成したイベント ID を使って、多様な種別 (Confirmed / WaitListed /
/// Cancelled / PromotedFromWaitList) のアクティビティを複数日にまたがって挿入する。
/// </remarks>
public sealed class AnalyticsDemoDataSeeder(
    IEventRepository eventRepository,
    IRegistrationActivityRepository activityRepository) : IDemoDataSeeder
{
    public int Order => 30;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // 冪等性: 既にアクティビティが存在するならスキップ
        var trackedIds = await activityRepository.GetTrackedEventIdsAsync(cancellationToken);
        if (trackedIds.Count > 0)
        {
            return;
        }

        var events = await eventRepository.GetAllAsync(cancellationToken);
        if (events.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // 各イベントについて、過去 14 日間に分散した活動ログを生成
        foreach (var ev in events)
        {
            // イベントの定員に応じたデータ量を決定
            var confirmedTotal = Math.Min(ev.Capacity, 25);
            var waitListedTotal = Math.Max(1, confirmedTotal / 4);
            var cancelledTotal = Math.Max(1, confirmedTotal / 5);
            var promotedTotal = Math.Max(1, cancelledTotal / 2);

            // 14 日間にわたって Confirmed を分散投入
            for (var i = 0; i < confirmedTotal; i++)
            {
                var daysAgo = 13 - (i * 13 / Math.Max(1, confirmedTotal - 1));
                var activity = RegistrationActivity.Create(
                    eventId: ev.Id,
                    registrationId: Guid.NewGuid(),
                    activityType: RegistrationActivityType.Confirmed,
                    occurredAt: now.AddDays(-daysAgo).AddHours(9 + (i % 8)));
                await activityRepository.AddAsync(activity, cancellationToken);
            }

            // WaitListed を中盤〜後半に分散
            for (var i = 0; i < waitListedTotal; i++)
            {
                var daysAgo = 8 - (i * 7 / Math.Max(1, waitListedTotal));
                var activity = RegistrationActivity.Create(
                    eventId: ev.Id,
                    registrationId: Guid.NewGuid(),
                    activityType: RegistrationActivityType.WaitListed,
                    occurredAt: now.AddDays(-daysAgo).AddHours(10 + (i % 6)));
                await activityRepository.AddAsync(activity, cancellationToken);
            }

            // Cancelled を後半に集中
            for (var i = 0; i < cancelledTotal; i++)
            {
                var daysAgo = 5 - (i * 4 / Math.Max(1, cancelledTotal));
                var activity = RegistrationActivity.Create(
                    eventId: ev.Id,
                    registrationId: Guid.NewGuid(),
                    activityType: RegistrationActivityType.Cancelled,
                    occurredAt: now.AddDays(-daysAgo).AddHours(14 + (i % 5)));
                await activityRepository.AddAsync(activity, cancellationToken);
            }

            // PromotedFromWaitList を Cancelled の直後に
            for (var i = 0; i < promotedTotal; i++)
            {
                var daysAgo = 4 - (i * 3 / Math.Max(1, promotedTotal));
                var activity = RegistrationActivity.Create(
                    eventId: ev.Id,
                    registrationId: Guid.NewGuid(),
                    activityType: RegistrationActivityType.PromotedFromWaitList,
                    occurredAt: now.AddDays(-daysAgo).AddHours(15 + (i % 4)));
                await activityRepository.AddAsync(activity, cancellationToken);
            }
        }

        await activityRepository.SaveChangesAsync(cancellationToken);
    }
}
