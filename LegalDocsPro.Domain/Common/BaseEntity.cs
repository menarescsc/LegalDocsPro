namespace LegalDocsPro.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities with audit fields and domain events support.
    /// </summary>
    public abstract class BaseEntity
    {
        private readonly List<DomainEvent> _domainEvents = new();

        public int Id { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Domain events raised by this entity.
        /// </summary>
        public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Adds a domain event to be dispatched.
        /// </summary>
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears all domain events (typically after dispatching).
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}