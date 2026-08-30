namespace LegalDocsPro.Domain.Common
{
    /// <summary>
    /// Base class for domain events. Domain events represent something that happened in the domain.
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public abstract string EventType { get; }
    }
}
