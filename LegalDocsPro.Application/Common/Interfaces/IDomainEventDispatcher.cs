using LegalDocsPro.Domain.Common;

namespace LegalDocsPro.Application.Common.Interfaces
{
    /// <summary>
    /// Dispatches domain events to their handlers.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a single domain event.
        /// </summary>
        Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispatches multiple domain events.
        /// </summary>
        Task DispatchAllAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
