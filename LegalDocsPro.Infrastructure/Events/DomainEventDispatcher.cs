using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Common;
using MediatR;

namespace LegalDocsPro.Infrastructure.Events
{
    /// <summary>
    /// Dispatches domain events using MediatR.
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IPublisher _publisher;

        public DomainEventDispatcher(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        public async Task DispatchAllAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                await DispatchAsync(domainEvent, cancellationToken);
            }
        }
    }
}
