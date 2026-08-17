using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class AttachContractDocumentCommandHandler : IRequestHandler<AttachContractDocumentCommand, Unit>
    {
        private readonly IContractRepository _repository;

        public AttachContractDocumentCommandHandler(IContractRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(AttachContractDocumentCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el contrato
            var contract = await _repository.GetByIdAsync(request.ContractId);

            if (contract == null)
                throw new KeyNotFoundException($"No se encontró el contrato con el ID {request.ContractId}.");

            // 2. Ejecutamos el comportamiento de dominio
            contract.AttachDocument(request.DocumentUrl);

            // 3. Guardamos los cambios
            await _repository.UpdateAsync(contract);

            return Unit.Value;
        }
    }
}