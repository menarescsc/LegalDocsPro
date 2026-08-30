using LegalDocsPro.Domain.Common;

namespace LegalDocsPro.Domain.Events
{
    public class ContractCreatedEvent : DomainEvent
    {
        public override string EventType => "contract.created";
        public int ContractId { get; }
        public string Title { get; }
        public string CreatedBy { get; }

        public ContractCreatedEvent(int contractId, string title, string createdBy)
        {
            ContractId = contractId;
            Title = title;
            CreatedBy = createdBy;
        }
    }

    public class ContractSentToReviewEvent : DomainEvent
    {
        public override string EventType => "contract.sent_to_review";
        public int ContractId { get; }

        public ContractSentToReviewEvent(int contractId)
        {
            ContractId = contractId;
        }
    }

    public class ContractApprovedEvent : DomainEvent
    {
        public override string EventType => "contract.approved";
        public int ContractId { get; }

        public ContractApprovedEvent(int contractId)
        {
            ContractId = contractId;
        }
    }

    public class ContractActivatedEvent : DomainEvent
    {
        public override string EventType => "contract.activated";
        public int ContractId { get; }

        public ContractActivatedEvent(int contractId)
        {
            ContractId = contractId;
        }
    }

    public class ContractDocumentAttachedEvent : DomainEvent
    {
        public override string EventType => "contract.document_attached";
        public int ContractId { get; }
        public string DocumentUrl { get; }

        public ContractDocumentAttachedEvent(int contractId, string documentUrl)
        {
            ContractId = contractId;
            DocumentUrl = documentUrl;
        }
    }
}
