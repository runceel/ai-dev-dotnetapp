using EventRegistration.Events.Application.Repositories;
using EventRegistration.Registrations.Application.UseCases;
using EventRegistration.SharedKernel.Application.DemoData;

namespace EventRegistration.Web.DemoData;

/// <summary>
/// Registrations モジュール用のデモデータ投入シーダー。
/// イベントごとに数名分の参加登録を投入する。
/// 既存のイベントが空、または既に登録が存在する場合は何もしない。
/// </summary>
public sealed class RegistrationsDemoDataSeeder(
    IEventRepository eventRepository,
    RegisterParticipantUseCase registerParticipantUseCase) : IDemoDataSeeder
{
    public int Order => 20;

    private static readonly (string Name, string Email)[] DemoParticipants =
    [
        ("山田 太郎", "taro.yamada@example.com"),
        ("鈴木 花子", "hanako.suzuki@example.com"),
        ("佐藤 次郎", "jiro.sato@example.com"),
        ("田中 三郎", "saburo.tanaka@example.com"),
        ("高橋 四郎", "shiro.takahashi@example.com"),
        ("伊藤 五郎", "goro.ito@example.com"),
        ("渡辺 六子", "rokuko.watanabe@example.com"),
        ("中村 七子", "nanako.nakamura@example.com"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var events = await eventRepository.GetAllAsync(cancellationToken);
        if (events.Count == 0)
        {
            return;
        }

        // 各イベントに対し、メールアドレスの重複チェック (HasActiveRegistrationAsync) が
        // 失敗した時点で何もしない (= 2 回目以降の起動はスキップ) ため、冪等性が確保される。
        // 投入数はイベントの定員 +1 (キャンセル待ちを 1 件作る) に上限を設ける。
        for (var eventIndex = 0; eventIndex < events.Count; eventIndex++)
        {
            var ev = events[eventIndex];
            var participantCount = Math.Min(ev.Capacity + 1, DemoParticipants.Length);

            for (var i = 0; i < participantCount; i++)
            {
                // メールが衝突しないよう、イベント単位でローテーションする
                var (name, email) = DemoParticipants[(i + eventIndex) % DemoParticipants.Length];
                var result = await registerParticipantUseCase.ExecuteAsync(
                    ev.Id, name, email, cancellationToken);

                // 失敗 (重複等) は無視: 2 回目以降の実行で発生する想定
                if (!result.IsSuccess)
                {
                    return;
                }
            }
        }
    }
}
