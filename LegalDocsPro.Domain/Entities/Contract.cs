using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Enums;
using LegalDocsPro.Domain.Events;
using LegalDocsPro.Domain.Exceptions;

namespace LegalDocsPro.Domain.Entities
{
    public class Contract : BaseEntity
    {
        // Properties with private setters to enforce encapsulation
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string ClientName { get; private set; } = string.Empty;
        public string DocumentUrl { get; private set; } = string.Empty;
        public new ContractStatus Status { get; private set; } = ContractStatus.Draft;
        public DateTime? EffectiveDate { get; private set; }
        public DateTime? ExpirationDate { get; private set; }

        // EF Core requires a parameterless constructor
        protected Contract() { }

        /// <summary>
        /// Creates a new contract with required fields.
        /// </summary>
        public Contract(string title, string description, string clientName, string documentUrl, DateTime? expirationDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Contract title is required.");

            Title = title;
            Description = description;
            ClientName = clientName;
            DocumentUrl = documentUrl;
            ExpirationDate = expirationDate;
            Status = ContractStatus.Draft;

            AddDomainEvent(new ContractCreatedEvent(Id, title, CreatedBy ?? "System"));
        }

        /// <summary>
        /// Transitions contract to InReview status.
        /// </summary>
        public void SendToReview()
        {
            if (Status != ContractStatus.Draft)
                throw new DomainException("Only draft contracts can be sent to review.");

            Status = ContractStatus.InReview;
            AddDomainEvent(new ContractSentToReviewEvent(Id));
        }

        /// <summary>
        /// Transitions contract to Approved status.
        /// </summary>
        public void Approve()
        {
            if (Status != ContractStatus.InReview)
                throw new DomainException("Only contracts in review can be approved.");

            Status = ContractStatus.Approved;
            AddDomainEvent(new ContractApprovedEvent(Id));
        }

        /// <summary>
        /// Transitions contract to Active status.
        /// </summary>
        public void Activate()
        {
            if (Status != ContractStatus.Approved)
                throw new DomainException("Contract must be approved before activation.");

            Status = ContractStatus.Active;
            EffectiveDate = DateTime.UtcNow;
            AddDomainEvent(new ContractActivatedEvent(Id));
        }

        /// <summary>
        /// Updates contract details (only allowed for Draft/InReview status).
        /// </summary>
        public void UpdateDetails(string title, string description)
        {
            if (Status == ContractStatus.Active || Status == ContractStatus.Expired)
                throw new DomainException("Cannot modify details of an active or expired contract.");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Contract title is required.");

            Title = title;
            Description = description;
        }

        /// <summary>
        /// Attaches a document to the contract (only allowed for Draft status).
        /// </summary>
        public void AttachDocument(string documentUrl)
        {
            if (Status != ContractStatus.Draft)
                throw new DomainException("Documents can only be attached to draft contracts.");

            if (string.IsNullOrWhiteSpace(documentUrl))
                throw new DomainException("Document URL is required.");

            DocumentUrl = documentUrl;
            AddDomainEvent(new ContractDocumentAttachedEvent(Id, documentUrl));
        }

        /// <summary>
        /// Updates the client name.
        /// </summary>
        public void UpdateClientName(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new DomainException("Client name is required.");

            ClientName = clientName;
        }
    }
}