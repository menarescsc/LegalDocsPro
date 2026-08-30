using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class AttachContractDocumentCommand : IRequest<Result>
    {
        public int ContractId { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;

        public AttachContractDocumentCommand(int contractId, string documentUrl)
        {
            ContractId = contractId;
            DocumentUrl = documentUrl;
        }
    }
}