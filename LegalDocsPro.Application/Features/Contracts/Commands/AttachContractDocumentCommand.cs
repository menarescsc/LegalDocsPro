using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    // 1. Asegúrate de que diga "public class"
    public class AttachContractDocumentCommand : IRequest<Unit>
    {
        public int ContractId { get; set; }
        public string DocumentUrl { get; set; }

        // 2. Asegúrate de que el constructor diga "public"
        public AttachContractDocumentCommand(int contractId, string documentUrl)
        {
            ContractId = contractId;
            DocumentUrl = documentUrl;
        }
    }
}