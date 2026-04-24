using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// 指定した ID のイベント詳細を取得するユースケース。
/// </summary>
public sealed class GetEventByIdUseCase
{
    private readonly IEventRepository _repository;

    public GetEventByIdUseCase(IEventRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 指定した ID のイベントを取得する。見つからない場合は null を返す。
    /// </summary>
    public async Task<Event?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
}
