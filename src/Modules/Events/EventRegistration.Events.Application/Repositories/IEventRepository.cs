using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.Repositories;

/// <summary>
/// Event エンティティのリポジトリ抽象。
/// </summary>
public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Event ev, CancellationToken cancellationToken = default);
}
